using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class PlaneacionTemplateService(AppDbContext context, IAutorizacionService autorizacion, IConfiguration configuration) : IPlaneacionTemplateService
{
    private const string DocxMime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public async Task<PlantillaPlaneacionDto> SubirAsync(Stream archivo, string nombreArchivo, string? mimeType, long usuarioId, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(usuarioId, RolAutorizacion.Director, cancellationToken);
        if (!string.Equals(Path.GetExtension(nombreArchivo), ".docx", StringComparison.OrdinalIgnoreCase)) throw new AppException("La plantilla debe estar en formato .docx.");
        if (!string.IsNullOrWhiteSpace(mimeType) && !string.Equals(mimeType, DocxMime, StringComparison.OrdinalIgnoreCase) && !string.Equals(mimeType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
            throw new AppException("El Content-Type de la plantilla no corresponde a un archivo DOCX.");
        await using var memory = new MemoryStream();
        await archivo.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();
        if (bytes.Length is 0 or > 25 * 1024 * 1024) throw new AppException("La plantilla debe tener un tamaño mayor a 0 y menor o igual a 25 MB.");
        if (!EsDocxValido(bytes)) throw new AppException("El archivo no es un documento DOCX válido.");

        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var directory = Path.Combine(RaizDocumentos, "plantillas-planeacion");
        Directory.CreateDirectory(directory);
        var savedName = $"{hash}.docx";
        var path = Path.Combine(directory, savedName);
        if (!File.Exists(path)) await File.WriteAllBytesAsync(path, bytes, cancellationToken);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var actual = await context.PlaneacionTemplates.Where(x => x.Activa).ToListAsync(cancellationToken);
        foreach (var template in actual) template.Activa = false;
        var version = (await context.PlaneacionTemplates.MaxAsync(x => (int?)x.Version, cancellationToken) ?? 0) + 1;
        var document = new Documento { TipoDocumento = TipoDocumento.PlantillaPlaneacion, Titulo = "Plantilla de planeación didáctica", NombreOriginal = Path.GetFileName(nombreArchivo), NombreGuardado = savedName, Extension = ".docx", MimeType = DocxMime, TamanoBytes = bytes.Length, RutaStorage = path, HashSha256 = hash, SubidoPorId = usuarioId, Estado = EstadoDocumento.Procesado };
        var templateNew = new PlaneacionTemplate { Documento = document, Version = version, Activa = true };
        context.PlaneacionTemplates.Add(templateNew);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Map(templateNew);
    }

    public async Task<PlantillaPlaneacionDto?> ObtenerActivaAsync(CancellationToken cancellationToken = default)
    {
        var template = await context.PlaneacionTemplates.Include(x => x.Documento).SingleOrDefaultAsync(x => x.Activa && x.Activo && x.DeletedAt == null, cancellationToken);
        return template is null ? null : Map(template);
    }

    public Task<ArchivoContenido> ObtenerArchivoAsync(Guid plantillaPublicId, CancellationToken cancellationToken = default) => ObtenerArchivoInternoAsync(context.PlaneacionTemplates.Include(x => x.Documento).SingleOrDefaultAsync(x => x.PublicId == plantillaPublicId && x.Activo && x.DeletedAt == null, cancellationToken));

    public Task<ArchivoContenido> ObtenerArchivoActivoAsync(CancellationToken cancellationToken = default) => ObtenerArchivoInternoAsync(context.PlaneacionTemplates.Include(x => x.Documento).SingleOrDefaultAsync(x => x.Activa && x.Activo && x.DeletedAt == null, cancellationToken));

    private static async Task<ArchivoContenido> ObtenerArchivoInternoAsync(Task<PlaneacionTemplate?> task)
    {
        var template = await task ?? throw new NotFoundException("No existe una plantilla activa de planeación.");
        if (!File.Exists(template.Documento.RutaStorage)) throw new AppException("El archivo de la plantilla no se encuentra disponible.");
        return new ArchivoContenido(await File.ReadAllBytesAsync(template.Documento.RutaStorage), template.Documento.MimeType, NombreSeguro(template.Documento.NombreOriginal, ".docx"));
    }

    private string RaizDocumentos => configuration["Almacenamiento:Raiz"] ?? Path.Combine(AppContext.BaseDirectory, "documentos");
    private static bool EsDocxValido(byte[] bytes)
    {
        try { using var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read); return zip.GetEntry("[Content_Types].xml") is not null && zip.GetEntry("word/document.xml") is not null; }
        catch (InvalidDataException) { return false; }
    }
    private static PlantillaPlaneacionDto Map(PlaneacionTemplate x) => new() { Id = x.PublicId, Nombre = x.Documento.NombreOriginal, Version = x.Version, Activa = x.Activa, FechaCarga = x.Documento.FechaSubida };
    internal static string NombreSeguro(string nombre, string extension) => $"{Regex.Replace(Path.GetFileNameWithoutExtension(nombre), "[^a-zA-Z0-9áéíóúÁÉÍÓÚñÑ _.-]", "_").Trim()}".TrimEnd('.') + extension;
}

public sealed class PlaneacionDocumentosService(AppDbContext context, IAutorizacionService autorizacion) : IPlaneacionDocumentosService
{
    public async Task<PlaneacionDetalleConArchivosDto> ObtenerDetalleAsync(Guid planeacionPublicId, long usuarioId, CancellationToken cancellationToken = default)
    {
        var p = await BuscarYAutorizarAsync(planeacionPublicId, usuarioId, cancellationToken);
        var program = p.Caratula?.ProgramaAsignatura;
        return new PlaneacionDetalleConArchivosDto { Planeacion = PlaneacionFlujoSupport.Detalle(p), Archivos = new PlaneacionArchivosDto
        {
            ProgramaAsignatura = program?.Documento is { } d && File.Exists(d.RutaStorage) ? new ArchivoRelacionadoDto { Disponible = true, Nombre = d.NombreOriginal, MimeType = "application/pdf", UrlVisualizacion = $"/api/programas-asignatura/{program.PublicId}/archivo", UrlDescarga = $"/api/programas-asignatura/{program.PublicId}/archivo/descarga" } : new(),
            PlaneacionDidactica = new ArchivoRelacionadoDto { Disponible = true, Nombre = PlaneacionTemplateService.NombreSeguro($"Planeacion_{p.Caratula?.NombreAsignatura ?? p.Asignatura.Nombre}.pdf", ".pdf"), MimeType = "application/pdf", UrlVisualizacion = $"/api/planeaciones-documentos/{p.PublicId}/pdf", UrlDescarga = $"/api/planeaciones-documentos/{p.PublicId}/pdf/descarga" }
        }};
    }

    public async Task<ArchivoContenido> ObtenerProgramaAsync(Guid programaPublicId, long usuarioId, CancellationToken cancellationToken = default)
    {
        var program = await context.ProgramasAsignatura.Include(x => x.Documento).SingleOrDefaultAsync(x => x.PublicId == programaPublicId && x.Activo && x.DeletedAt == null, cancellationToken) ?? throw new NotFoundException("El programa de asignatura no existe.");
        var planeacion = await context.PlaneacionesDidacticas.Include(x => x.Caratula).FirstOrDefaultAsync(x => x.Caratula!.ProgramaAsignaturaId == program.Id && x.Activo && x.DeletedAt == null, cancellationToken) ?? throw new UnauthorizedAccessException("No tiene una planeación asociada a este programa.");
        await AutorizarAsync(planeacion, usuarioId, cancellationToken);
        if (!File.Exists(program.Documento.RutaStorage)) throw new AppException("El archivo del programa no se encuentra disponible.");
        return new ArchivoContenido(await File.ReadAllBytesAsync(program.Documento.RutaStorage, cancellationToken), "application/pdf", PlaneacionTemplateService.NombreSeguro(program.Documento.NombreOriginal, ".pdf"));
    }

    internal async Task<PlaneacionDidactica> BuscarYAutorizarAsync(Guid publicId, long usuarioId, CancellationToken ct)
    {
        var p = await PlaneacionFlujoSupport.QueryDetalle(context).Include(x => x.Caratula!).ThenInclude(x => x.ProgramaAsignatura!).ThenInclude(x => x.Documento).SingleOrDefaultAsync(x => x.PublicId == publicId && x.Activo && x.DeletedAt == null, ct) ?? throw new NotFoundException("La planeación solicitada no existe.");
        await AutorizarAsync(p, usuarioId, ct); return p;
    }
    internal async Task AutorizarAsync(PlaneacionDidactica p, long userId, CancellationToken ct)
    {
        if (await autorizacion.HasRoleAsync(userId, RolAutorizacion.Director, ct)) return;
        if (await autorizacion.HasRoleAsync(userId, RolAutorizacion.Docente, ct))
        {
            var assigned = await context.CargasAcademicas.AnyAsync(x => x.Activo && x.DeletedAt == null && x.DocenteId == userId && x.PeriodoId == p.PeriodoId && x.AsignaturaId == p.AsignaturaId, ct);
            if (assigned) return;
        }
        if (await autorizacion.HasRoleAsync(userId, RolAutorizacion.Revisor, ct) && p.RevisorId == userId) return;
        throw new UnauthorizedAccessException("No tiene permiso efectivo para consultar esta planeación.");
    }
}
