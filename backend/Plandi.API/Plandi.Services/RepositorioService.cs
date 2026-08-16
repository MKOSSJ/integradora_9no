using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class RepositorioService(
    AppDbContext context,
    IAutorizacionService autorizacion,
    IPeriodoLifecycleService lifecycle,
    IRelojAcademico reloj,
    IPlaneacionPdfService pdf) : IRepositorioService
{
    public async Task<PagedResult<RepositorioPlaneacionDto>> BuscarAsync(RepositorioPlaneacionesFiltroDto f, long usuarioId, CancellationToken ct = default)
    {
        await ExigirRolConAccesoAsync(usuarioId, ct);
        var hoy = reloj.AhoraLocal.Date;
        var q = context.PlaneacionesDidacticas.AsNoTracking().Where(p => p.Activo && p.DeletedAt == null && p.Periodo.DeletedAt == null &&
            (p.Periodo.Estado == EstadoPeriodo.Cerrado || (p.Periodo.FechaFin != default && p.Periodo.FechaFin.Date < hoy)));

        q = await AplicarAccesoAsync(q, usuarioId, ct);
        if (f.PeriodoPublicId.HasValue) q = q.Where(p => p.Periodo.PublicId == f.PeriodoPublicId);
        if (f.AsignaturaPublicId.HasValue) q = q.Where(p => p.Asignatura.PublicId == f.AsignaturaPublicId);
        if (f.DocentePublicId.HasValue) q = q.Where(p => context.CargasAcademicas.Any(c => c.DeletedAt == null && c.PeriodoId == p.PeriodoId && c.AsignaturaId == p.AsignaturaId && c.Docente.PublicId == f.DocentePublicId));
        if (f.CicloPublicId.HasValue) q = q.Where(p => p.Periodo.CicloEscolar.PublicId == f.CicloPublicId);
        if (f.GrupoPublicId.HasValue) q = q.Where(p => context.CargasAcademicas.Any(c => c.DeletedAt == null && c.PeriodoId == p.PeriodoId && c.AsignaturaId == p.AsignaturaId && c.Grupo.PublicId == f.GrupoPublicId));
        if (f.CarreraPublicId.HasValue) q = q.Where(p => context.CargasAcademicas.Any(c => c.DeletedAt == null && c.PeriodoId == p.PeriodoId && c.AsignaturaId == p.AsignaturaId && c.Grupo.Carrera.PublicId == f.CarreraPublicId));
        if (f.AcademiaPublicId.HasValue) q = q.Where(p => p.Academia != null && p.Academia.PublicId == f.AcademiaPublicId);
        if (f.EstadoPlaneacion.HasValue) q = q.Where(p => p.Estado == f.EstadoPlaneacion);
        if (!string.IsNullOrWhiteSpace(f.Search)) { var s = f.Search.Trim(); q = q.Where(p => p.Asignatura.Nombre.Contains(s) || p.Asignatura.Clave.Contains(s) || p.Periodo.Nombre.Contains(s)); }

        var total = await q.CountAsync(ct);
        var items = await DetalleQuery(q).OrderByDescending(p => p.Periodo.FechaFin).ThenBy(p => p.Asignatura.Nombre)
            .Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync(ct);
        var cargas = await CargasAsync(items, ct);
        return new PagedResult<RepositorioPlaneacionDto> { Items = items.Select(p => Map(p, cargas.GetValueOrDefault((p.PeriodoId, p.AsignaturaId), []))).ToList(), Page = f.Page, PageSize = f.PageSize, TotalItems = total };
    }

    public async Task<RepositorioPlaneacionDto> ObtenerAsync(Guid id, long usuarioId, CancellationToken ct = default)
    {
        var p = await BuscarAutorizadaAsync(id, usuarioId, ct);
        var cargas = await CargasAsync([p], ct);
        return Map(p, cargas.GetValueOrDefault((p.PeriodoId, p.AsignaturaId), []));
    }

    public async Task<IReadOnlyList<RepositorioArchivoDto>> ArchivosAsync(Guid id, long usuarioId, CancellationToken ct = default) =>
        (await ObtenerAsync(id, usuarioId, ct)).Archivos;

    public async Task<ArchivoContenido> DescargarAsync(Guid id, string tipo, long usuarioId, CancellationToken ct = default)
    {
        var p = await BuscarAutorizadaAsync(id, usuarioId, ct);
        if (string.Equals(tipo, "planeacion", StringComparison.OrdinalIgnoreCase))
            return await pdf.GenerarPdfAsync(id, usuarioId, ct);
        if (!string.Equals(tipo, "programa", StringComparison.OrdinalIgnoreCase))
            throw new NotFoundException("El tipo de archivo solicitado no existe.");

        var documento = p.Caratula?.ProgramaAsignatura?.Documento ?? throw new NotFoundException("La planeación no tiene un programa de asignatura asociado.");
        if (!documento.Activo || documento.DeletedAt is not null || !File.Exists(documento.RutaStorage))
            throw new NotFoundException("El archivo del programa de asignatura no se encuentra disponible.");
        return new ArchivoContenido(await File.ReadAllBytesAsync(documento.RutaStorage, ct), documento.MimeType, PlaneacionTemplateService.NombreSeguro(documento.NombreOriginal, documento.Extension));
    }

    private async Task<PlaneacionDidactica> BuscarAutorizadaAsync(Guid id, long usuarioId, CancellationToken ct)
    {
        await ExigirRolConAccesoAsync(usuarioId, ct);
        var q = await AplicarAccesoAsync(DetalleQuery(context.PlaneacionesDidacticas.AsNoTracking().Where(p => p.PublicId == id && p.Activo && p.DeletedAt == null)), usuarioId, ct);
        var p = await q.SingleOrDefaultAsync(ct) ?? throw new NotFoundException("La planeación histórica no existe o no tiene acceso a ella.");
        if (lifecycle.ObtenerEstadoEfectivo(p.Periodo) != EstadoPeriodo.Cerrado)
            throw new ConflictException("La planeación pertenece a un periodo vigente y todavía no forma parte del repositorio.");
        return p;
    }

    private async Task<IQueryable<PlaneacionDidactica>> AplicarAccesoAsync(IQueryable<PlaneacionDidactica> q, long userId, CancellationToken ct)
    {
        if (await autorizacion.HasRoleAsync(userId, RolAutorizacion.Director, ct)) return q;
        var docente = await autorizacion.HasRoleAsync(userId, RolAutorizacion.Docente, ct);
        var revisor = await autorizacion.HasRoleAsync(userId, RolAutorizacion.Revisor, ct);
        return q.Where(p => (revisor && p.RevisorId == userId) || (docente && context.CargasAcademicas.Any(c => c.DeletedAt == null && c.DocenteId == userId && c.PeriodoId == p.PeriodoId && c.AsignaturaId == p.AsignaturaId)));
    }

    private async Task ExigirRolConAccesoAsync(long userId, CancellationToken ct)
    {
        if (!await autorizacion.HasRoleAsync(userId, RolAutorizacion.Director, ct) &&
            !await autorizacion.HasRoleAsync(userId, RolAutorizacion.Docente, ct) &&
            !await autorizacion.HasRoleAsync(userId, RolAutorizacion.Revisor, ct))
            throw new ForbiddenException("El usuario no tiene un rol con acceso al repositorio.");
    }

    private static IQueryable<PlaneacionDidactica> DetalleQuery(IQueryable<PlaneacionDidactica> q) => q
        .Include(p => p.Periodo).ThenInclude(p => p.CicloEscolar).Include(p => p.Asignatura).Include(p => p.Academia)
        .Include(p => p.Caratula!).ThenInclude(c => c.ProgramaAsignatura!).ThenInclude(p => p.Documento).AsSplitQuery();

    private async Task<Dictionary<(long, long), List<CargaAcademica>>> CargasAsync(IEnumerable<PlaneacionDidactica> planes, CancellationToken ct)
    {
        var pairs = planes.Select(p => (p.PeriodoId, p.AsignaturaId)).Distinct().ToList();
        if (pairs.Count == 0) return [];
        var periodos = pairs.Select(x => x.PeriodoId).Distinct().ToList();
        var asignaturas = pairs.Select(x => x.AsignaturaId).Distinct().ToList();
        var cargas = await context.CargasAcademicas.AsNoTracking().Where(c => c.DeletedAt == null && periodos.Contains(c.PeriodoId) && asignaturas.Contains(c.AsignaturaId))
            .Include(c => c.Docente).Include(c => c.Grupo).ThenInclude(g => g.Carrera).ToListAsync(ct);
        return cargas.Where(c => pairs.Contains((c.PeriodoId, c.AsignaturaId))).GroupBy(c => (c.PeriodoId, c.AsignaturaId)).ToDictionary(g => g.Key, g => g.ToList());
    }

    private RepositorioPlaneacionDto Map(PlaneacionDidactica p, List<CargaAcademica> cargas)
    {
        var programa = p.Caratula?.ProgramaAsignatura;
        var documento = programa?.Documento;
        var nombrePdf = PlaneacionTemplateService.NombreSeguro($"Planeacion_{p.Asignatura.Nombre}", ".pdf");
        return new RepositorioPlaneacionDto
        {
            PublicId = p.PublicId,
            Asignatura = new AdminEntidadResumenDto { PublicId = p.Asignatura.PublicId, Nombre = p.Asignatura.Nombre, Clave = p.Asignatura.Clave },
            Periodo = new AdminPeriodoResumenDto { PublicId = p.Periodo.PublicId, Nombre = p.Periodo.Nombre, FechaInicio = p.Periodo.FechaInicio, FechaFin = p.Periodo.FechaFin, Estado = lifecycle.ObtenerEstadoEfectivo(p.Periodo), PermiteModificaciones = false },
            Ciclo = new AdminEntidadResumenDto { PublicId = p.Periodo.CicloEscolar.PublicId, Nombre = p.Periodo.CicloEscolar.Nombre },
            Docentes = cargas.Select(c => new AdminEntidadResumenDto { PublicId = c.Docente.PublicId, Nombre = Nombre(c.Docente) }).DistinctBy(x => x.PublicId).ToList(),
            Grupos = cargas.Select(c => new AdminEntidadResumenDto { PublicId = c.Grupo.PublicId, Nombre = c.Grupo.Nombre }).DistinctBy(x => x.PublicId).ToList(),
            Carreras = cargas.Select(c => new AdminEntidadResumenDto { PublicId = c.Grupo.Carrera.PublicId, Nombre = c.Grupo.Carrera.Nombre, Clave = c.Grupo.Carrera.Clave }).DistinctBy(x => x.PublicId).ToList(),
            Academia = p.Academia is null ? null : new AdminEntidadResumenDto { PublicId = p.Academia.PublicId, Nombre = p.Academia.Nombre },
            EstadoPlaneacion = p.Estado, Fecha = p.UpdatedAt ?? p.CreatedAt, SoloLectura = true,
            Archivos =
            [
                new RepositorioArchivoDto { Tipo = "planeacion", Nombre = nombrePdf, MimeType = "application/pdf", Disponible = true, UrlDescarga = $"/api/repositorio/planeaciones/{p.PublicId}/archivos/planeacion/descargar" },
                new RepositorioArchivoDto { Tipo = "programa", Nombre = documento?.NombreOriginal ?? "Programa de asignatura.pdf", MimeType = documento?.MimeType ?? "application/pdf", TamanoBytes = documento?.TamanoBytes, Disponible = documento is not null && documento.Activo && documento.DeletedAt is null && File.Exists(documento.RutaStorage), UrlDescarga = $"/api/repositorio/planeaciones/{p.PublicId}/archivos/programa/descargar" }
            ]
        };
    }

    private static string Nombre(Usuario u) => string.Join(" ", new[] { u.Nombre, u.ApellidoPaterno, u.ApellidoMaterno }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
