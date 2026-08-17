using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Enums;
using Plandi.Dto.Resumen;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class ResumenService(AppDbContext context, IAutorizacionService autorizacion, IRelojAcademico reloj) : IResumenService
{
    public async Task<ResumenDashboardDto> ObtenerDashboardAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = context.Usuarios.AsNoTracking().Where(x => x.DeletedAt == null);
        var academias = context.Academias.AsNoTracking().Where(x => x.Activo && x.DeletedAt == null);
        var grupos = context.Grupos.AsNoTracking().Where(x => x.Activo && x.DeletedAt == null);
        // "Importaciones" no tiene historial persistente: representa las filas vigentes o inactivas de carga académica.
        var cargas = context.CargasAcademicas.AsNoTracking().Where(x => x.DeletedAt == null);
        var planeaciones = context.PlaneacionesDidacticas.AsNoTracking().Where(x => x.Activo && x.DeletedAt == null);

        var totalUsuarios = await usuarios.CountAsync(cancellationToken);
        var totalAcademias = await academias.CountAsync(cancellationToken);
        var totalGrupos = await grupos.CountAsync(cancellationToken);
        var totalCargas = await cargas.CountAsync(cancellationToken);
        var conteoPlaneaciones = await planeaciones.GroupBy(_ => 1)
            .Select(g => new ConteoPlaneaciones(g.Count(), g.Count(x => x.Estado == EstadoPlaneacion.Aprobada)))
            .SingleOrDefaultAsync(cancellationToken);
        var avance = conteoPlaneaciones is null || conteoPlaneaciones.Total == 0
            ? 0m
            : decimal.Round(Math.Clamp(conteoPlaneaciones.Aprobadas * 100m / conteoPlaneaciones.Total, 0m, 100m), 2);

        return new ResumenDashboardDto
        {
            UsuariosRegistrados = totalUsuarios, Academias = totalAcademias, GruposActivos = totalGrupos,
            Importaciones = totalCargas, AvancePlaneaciones = avance
        };
    }

    public async Task<ResumenUsuariosDto> ObtenerUsuariosAsync(CancellationToken cancellationToken = default)
    {
        var conteo = await context.Usuarios.AsNoTracking().Where(x => x.DeletedAt == null)
            .GroupBy(_ => 1)
            .Select(g => new ConteoUsuarios(
                g.Count(),
                g.Count(x => x.UsuarioRoles.Any(r => r.Rol.Activo && r.Rol.DeletedAt == null && r.Rol.Nombre == "Docente")),
                g.Count(x => x.UsuarioRoles.Any(r => r.Rol.Activo && r.Rol.DeletedAt == null && r.Rol.Nombre == "Revisor")),
                g.Count(x => x.UsuarioRoles.Any(r => r.Rol.Activo && r.Rol.DeletedAt == null && r.Rol.Nombre == "Director"))))
            .SingleOrDefaultAsync(cancellationToken);
        return conteo is null ? new ResumenUsuariosDto() : new ResumenUsuariosDto { Total = conteo.Total, Docentes = conteo.Docentes, Revisores = conteo.Revisores, Directores = conteo.Directores };
    }

    public async Task<ResumenCarrerasDto> ObtenerCarrerasAsync(CancellationToken cancellationToken = default)
    {
        var c = await ContarPorEstadoAsync(context.Carreras.AsNoTracking(), cancellationToken);
        return new ResumenCarrerasDto { Total = c.Total, Activas = c.Activos, Inactivas = c.Inactivos };
    }

    public async Task<ResumenAsignaturasDto> ObtenerAsignaturasAsync(CancellationToken cancellationToken = default)
    {
        var c = await ContarPorEstadoAsync(context.Asignaturas.AsNoTracking(), cancellationToken);
        return new ResumenAsignaturasDto { Total = c.Total, Activas = c.Activos, Inactivas = c.Inactivos };
    }

    public async Task<ResumenCiclosEscolaresDto> ObtenerCiclosEscolaresAsync(CancellationToken cancellationToken = default)
    {
        var c = await ContarPorEstadoAsync(context.CiclosEscolares.AsNoTracking(), cancellationToken);
        return new ResumenCiclosEscolaresDto { Total = c.Total, Activos = c.Activos, Inactivos = c.Inactivos };
    }

    public async Task<ResumenPeriodosDto> ObtenerPeriodosAsync(CancellationToken cancellationToken = default)
    {
        var c = await ContarPorEstadoAsync(context.Periodos.AsNoTracking(), cancellationToken);
        return new ResumenPeriodosDto { Total = c.Total, Activos = c.Activos, Inactivos = c.Inactivos };
    }

    public async Task<ResumenGruposDto> ObtenerGruposAsync(CancellationToken cancellationToken = default)
    {
        var c = await ContarPorEstadoAsync(context.Grupos.AsNoTracking(), cancellationToken);
        return new ResumenGruposDto { Total = c.Total, Activos = c.Activos, Inactivos = c.Inactivos };
    }

    public async Task<ResumenAsignacionAcademicaDto> ObtenerAsignacionAcademicaAsync(CancellationToken cancellationToken = default)
    {
        var c = await ContarPorEstadoAsync(context.CargasAcademicas.AsNoTracking(), cancellationToken);
        return new ResumenAsignacionAcademicaDto { Total = c.Total, Activas = c.Activos, Inactivas = c.Inactivos };
    }

    public async Task<ResumenSeguimientoPlaneacionesDto> ObtenerSeguimientoPlaneacionesAsync(CancellationToken cancellationToken = default)
    {
        var hoy = reloj.AhoraLocal.Date;
        var conteo = await PlaneacionesDelPeriodoActual(hoy)
            .GroupBy(_ => 1)
            .Select(g => new ConteoSeguimiento(
                g.Count(),
                g.Count(p => p.Estado == EstadoPlaneacion.Aprobada),
                g.Count(p => p.Estado == EstadoPlaneacion.EnRevision),
                g.Count(p => p.Estado != EstadoPlaneacion.Aprobada && p.Periodo.FechaLimiteEntregaPlaneaciones.HasValue && hoy <= p.Periodo.FechaLimiteEntregaPlaneaciones.Value)))
            .SingleOrDefaultAsync(cancellationToken);
        return conteo is null ? new ResumenSeguimientoPlaneacionesDto() : new ResumenSeguimientoPlaneacionesDto
        {
            Total = conteo.Total, Completadas = conteo.Completadas, EnRevision = conteo.EnRevision, PorVencer = conteo.PorVencer
        };
    }

    public async Task<ResumenDashboardDocenteDto> ObtenerDashboardDocenteAsync(long docenteId, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(docenteId, RolAutorizacion.Docente, cancellationToken);
        var conteo = await PlaneacionesDelDocente(docenteId).GroupBy(_ => 1)
            .Select(g => new ConteoDocente(g.Count(), g.Count(p => p.Estado == EstadoPlaneacion.Aprobada),
                g.Count(p => p.Estado == EstadoPlaneacion.Borrador || p.Estado == EstadoPlaneacion.EnProceso || p.Estado == EstadoPlaneacion.EnRevision || p.Estado == EstadoPlaneacion.CorreccionSolicitada || p.Estado == EstadoPlaneacion.Reabierta)))
            .SingleOrDefaultAsync(cancellationToken);
        return conteo is null ? new ResumenDashboardDocenteDto() : new ResumenDashboardDocenteDto { Planeaciones = conteo.Total, Aprobadas = conteo.Aprobadas, Pendientes = conteo.Pendientes };
    }

    public async Task<ResumenPlaneacionesDocenteDto> ObtenerPlaneacionesDocenteAsync(long docenteId, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(docenteId, RolAutorizacion.Docente, cancellationToken);
        var conteo = await PlaneacionesDelDocente(docenteId).GroupBy(_ => 1)
            // CorreccionSolicitada y Reabierta se agrupan en Borrador: son estados editables del flujo docente.
            .Select(g => new ConteoPlaneacionesDocente(g.Count(),
                g.Count(p => p.Estado == EstadoPlaneacion.Borrador || p.Estado == EstadoPlaneacion.EnProceso || p.Estado == EstadoPlaneacion.CorreccionSolicitada || p.Estado == EstadoPlaneacion.Reabierta),
                g.Count(p => p.Estado == EstadoPlaneacion.EnRevision),
                g.Count(p => p.Estado == EstadoPlaneacion.Aprobada)))
            .SingleOrDefaultAsync(cancellationToken);
        return conteo is null ? new ResumenPlaneacionesDocenteDto() : new ResumenPlaneacionesDocenteDto { Total = conteo.Total, Borrador = conteo.Borrador, Revision = conteo.Revision, Aprobadas = conteo.Aprobadas };
    }

    public async Task<ResumenDashboardRevisorDto> ObtenerDashboardRevisorAsync(long revisorId, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(revisorId, RolAutorizacion.Revisor, cancellationToken);
        var conteo = await PlaneacionesVisiblesParaRevisor(revisorId).GroupBy(_ => 1)
            .Select(g => new ConteoRevisor(g.Count(), g.Count(p => p.Estado == EstadoPlaneacion.Aprobada),
                g.Count(p => p.Estado == EstadoPlaneacion.CorreccionSolicitada), g.Count(p => p.Estado == EstadoPlaneacion.EnRevision)))
            .SingleOrDefaultAsync(cancellationToken);
        return conteo is null ? new ResumenDashboardRevisorDto() : new ResumenDashboardRevisorDto { Planeaciones = conteo.Total, Validadas = conteo.Validadas, Correcciones = conteo.Correcciones, PlaneacionesAValidar = conteo.Pendientes };
    }

    public async Task<ResumenValidacionDto> ObtenerValidacionAsync(long revisorId, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(revisorId, RolAutorizacion.Revisor, cancellationToken);
        var conteo = await PlaneacionesVisiblesParaRevisor(revisorId).GroupBy(_ => 1)
            .Select(g => new ConteoValidacion(g.Count(p => p.Estado == EstadoPlaneacion.EnRevision),
                g.Count(p => p.Estado == EstadoPlaneacion.Aprobada), g.Count(p => p.Estado == EstadoPlaneacion.CorreccionSolicitada)))
            .SingleOrDefaultAsync(cancellationToken);
        return conteo is null ? new ResumenValidacionDto() : new ResumenValidacionDto { Pendientes = conteo.Pendientes, Aprobadas = conteo.Aprobadas, Correcciones = conteo.Correcciones };
    }

    private IQueryable<PlaneacionDidactica> PlaneacionesDelDocente(long docenteId) => context.PlaneacionesDidacticas.AsNoTracking()
        .Where(p => p.Activo && p.DeletedAt == null && context.CargasAcademicas.Any(c =>
            c.Activo && c.DeletedAt == null && c.DocenteId == docenteId && c.PeriodoId == p.PeriodoId && c.AsignaturaId == p.AsignaturaId));

    private IQueryable<PlaneacionDidactica> PlaneacionesVisiblesParaRevisor(long revisorId) => context.PlaneacionesDidacticas.AsNoTracking()
        .Where(p => p.Activo && p.DeletedAt == null && p.RevisorId == revisorId && p.Estado != EstadoPlaneacion.Borrador && p.Estado != EstadoPlaneacion.EnProceso);

    private IQueryable<PlaneacionDidactica> PlaneacionesDelPeriodoActual(DateTime hoy) => context.PlaneacionesDidacticas.AsNoTracking()
        .Where(p => p.Activo && p.DeletedAt == null && p.Periodo.Activo && p.Periodo.DeletedAt == null &&
            p.Periodo.Estado != EstadoPeriodo.Cerrado && (p.Periodo.FechaInicio == default || p.Periodo.FechaInicio <= hoy) &&
            (p.Periodo.FechaFin == default || p.Periodo.FechaFin >= hoy));

    private static async Task<ConteoEstado> ContarPorEstadoAsync<TEntity>(IQueryable<TEntity> query, CancellationToken cancellationToken) where TEntity : BaseEntity
    {
        var conteo = await query.Where(x => x.DeletedAt == null).GroupBy(_ => 1)
            .Select(g => new ConteoEstado(g.Count(), g.Count(x => x.Activo), g.Count(x => !x.Activo)))
            .SingleOrDefaultAsync(cancellationToken);
        return conteo ?? new ConteoEstado(0, 0, 0);
    }

    private sealed record ConteoEstado(int Total, int Activos, int Inactivos);
    private sealed record ConteoPlaneaciones(int Total, int Aprobadas);
    private sealed record ConteoUsuarios(int Total, int Docentes, int Revisores, int Directores);
    private sealed record ConteoSeguimiento(int Total, int Completadas, int EnRevision, int PorVencer);
    private sealed record ConteoDocente(int Total, int Aprobadas, int Pendientes);
    private sealed record ConteoPlaneacionesDocente(int Total, int Borrador, int Revision, int Aprobadas);
    private sealed record ConteoRevisor(int Total, int Validadas, int Correcciones, int Pendientes);
    private sealed record ConteoValidacion(int Pendientes, int Aprobadas, int Correcciones);
}
