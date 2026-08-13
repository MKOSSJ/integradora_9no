using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;
using Plandi.Services.ProgramaAsignaturaExtraction;

namespace Plandi.Services;

public sealed class ProgramaAsignaturaImportService(
    AppDbContext dbContext,
    ILogger<ProgramaAsignaturaImportService> logger,
    IPdfTextExtractor pdfTextExtractor,
    ProgramaAsignaturaExtractor programaExtractor) : IProgramaAsignaturaImportService
{
    public async Task<string> ExtraerTextoAsync(Stream archivo, string nombreArchivo, CancellationToken cancellationToken = default)
    {
        var bytes = await ReadAndValidatePdfAsync(archivo, nombreArchivo, cancellationToken);
        return ExtractRawText(bytes, nombreArchivo);
    }

    public async Task<ProgramaAsignaturaImportacionResultadoDto> ImportarAsync(Stream archivo, string nombreArchivo,
        long tamanoBytes, string? mimeType, Guid subidoPorPublicId, string directorioStorage, CancellationToken cancellationToken = default)
    {
        var result = new ProgramaAsignaturaImportacionResultadoDto { Archivo = nombreArchivo };
        var user = await dbContext.Usuarios.SingleOrDefaultAsync(user => user.PublicId == subidoPorPublicId && user.Activo && user.DeletedAt == null, cancellationToken)
            ?? throw new AppException("El usuario que realiza la carga no existe.");
        var bytes = await ReadAndValidatePdfAsync(archivo, nombreArchivo, cancellationToken);
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var duplicate = await dbContext.Documentos.Include(document => document.ProgramaAsignatura)
            .FirstOrDefaultAsync(document => document.HashSha256 == hash && document.Activo && document.DeletedAt == null, cancellationToken);
        if (duplicate?.ProgramaAsignatura is not null)
        {
            result.ProgramaAsignaturaPublicId = duplicate.ProgramaAsignatura.PublicId;
            result.Asignatura = duplicate.ProgramaAsignatura.NombreAsignatura;
            result.Clave = duplicate.ProgramaAsignatura.ClaveAsignatura;
            result.DatosGuardados = true;
            return result;
        }

        var documentText = ExtractDocument(bytes, nombreArchivo);
        var rawText = documentText.RawText;
        var extracted = programaExtractor.Extract(documentText);
        if (string.IsNullOrWhiteSpace(extracted.NombreAsignatura) || string.IsNullOrWhiteSpace(extracted.ClaveAsignatura))
            throw new AppException("No se identificaron la asignatura y la clave requeridas en el PDF.");

        var subject = await FindOrCreateSubjectAsync(extracted, cancellationToken);
        Directory.CreateDirectory(directorioStorage);
        var savedName = $"{hash}.pdf";
        var fullPath = Path.Combine(directorioStorage, savedName);
        if (!File.Exists(fullPath)) await File.WriteAllBytesAsync(fullPath, bytes, cancellationToken);

        var document = new Documento
        {
            TipoDocumento = TipoDocumento.ProgramaAsignatura,
            Titulo = $"Programa de Asignatura - {extracted.NombreAsignatura}",
            NombreOriginal = nombreArchivo,
            NombreGuardado = savedName,
            Extension = ".pdf",
            MimeType = string.IsNullOrWhiteSpace(mimeType) ? "application/pdf" : mimeType,
            TamanoBytes = tamanoBytes,
            RutaStorage = fullPath,
            HashSha256 = hash,
            SubidoPorId = user.Id,
            Estado = EstadoDocumento.Procesado
        };

        ProgramaAsignatura? program = null;
        if (subject.Id > 0)
        {
            program = await dbContext.ProgramasAsignatura.FirstOrDefaultAsync(program => program.Activo && program.DeletedAt == null &&
                program.AsignaturaId == subject.Id && program.Cuatrimestre == extracted.Cuatrimestre, cancellationToken);
        }
        if (program is null)
        {
            program = new ProgramaAsignatura();
            dbContext.ProgramasAsignatura.Add(program);
        }
        else program.UpdatedAt = DateTime.UtcNow;

        program.Documento = document;
        // For a newly extracted subject its database Id is still zero. Assigning
        // the navigation lets EF persist the subject first and set the FK safely.
        program.Asignatura = subject;
        program.NombreAsignatura = extracted.NombreAsignatura;
        program.ClaveAsignatura = extracted.ClaveAsignatura;
        program.Carrera = extracted.ProgramaEducativo;
        program.Cuatrimestre = extracted.Cuatrimestre;
        program.Competencia = extracted.Competencia;
        program.Proposito = extracted.Proposito;
        program.Creditos = extracted.Creditos;
        program.HorasTotales = extracted.HorasTotales;
        program.HorasSemana = extracted.HorasSemana;
        program.TextoExtraido = rawText;
        program.JsonExtraido = JsonSerializer.Serialize(extracted);
        program.UltimaModificacionPorId = user.Id;
        program.FechaUltimaModificacion = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Programa {Clave} importado desde {Archivo}", extracted.ClaveAsignatura, nombreArchivo);
        result.ProgramaAsignaturaPublicId = program.PublicId;
        result.Asignatura = program.NombreAsignatura;
        result.Clave = program.ClaveAsignatura;
        result.UnidadesExtraidas = extracted.Unidades.Count;
        result.DatosGuardados = true;
        return result;
    }

    private string ExtractRawText(byte[] bytes, string fileName)
        => ExtractDocument(bytes, fileName).RawText;

    private PdfProgramDocument ExtractDocument(byte[] bytes, string fileName)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            return pdfTextExtractor.ExtractDocument(stream);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "No fue posible extraer el PDF {Archivo}", fileName);
            throw new AppException("No fue posible leer el contenido del PDF.");
        }
    }

    private static async Task<byte[]> ReadAndValidatePdfAsync(Stream file, string fileName, CancellationToken cancellationToken)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Solo se admiten programas de asignatura en formato PDF.");
        await using var memory = new MemoryStream();
        await file.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();
        if (bytes.Length == 0 || !Encoding.ASCII.GetString(bytes, 0, Math.Min(bytes.Length, 5)).StartsWith("%PDF-"))
            throw new AppException("El archivo no es un PDF valido.");
        return bytes;
    }

    private async Task<Asignatura> FindOrCreateSubjectAsync(ProgramaAsignaturaExtraidoDto data, CancellationToken cancellationToken)
    {
        var normalizedName = Normalize(data.NombreAsignatura);
        var candidates = await dbContext.Asignaturas.Where(subject => subject.Activo && subject.DeletedAt == null).ToListAsync(cancellationToken);
        var existing = candidates.FirstOrDefault(subject => Normalize(subject.Nombre) == normalizedName || string.Equals(subject.Clave, data.ClaveAsignatura, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) return existing;

        var subject = new Asignatura
        {
            Nombre = data.NombreAsignatura,
            Clave = data.ClaveAsignatura,
            Cuatrimestre = data.Cuatrimestre ?? 1,
            HorasTotales = data.HorasTotales ?? 0,
            HorasSemana = data.HorasSemana ?? 0,
            Creditos = data.Creditos ?? 0
        };
        dbContext.Asignaturas.Add(subject);
        return subject;
    }

    private static string Normalize(string value) => new string(SectionReader.Clean(value).ToUpperInvariant().Normalize(NormalizationForm.FormD)
        .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark).ToArray());
}
