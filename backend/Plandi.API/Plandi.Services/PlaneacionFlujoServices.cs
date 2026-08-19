using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class MisPlaneacionesDocenteService(AppDbContext context, IAutorizacionService autorizacion) : IMisPlaneacionesDocenteService
{
    public async Task<IReadOnlyList<PlaneacionResumenDto>> ObtenerAsync(long docenteId, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(docenteId, RolAutorizacion.Docente, cancellationToken);
        var planeaciones = await PlaneacionFlujoSupport.QueryDetalle(context)
            .Where(p => p.Activo && p.DeletedAt == null && context.CargasAcademicas.Any(c =>
                c.Activo && c.DeletedAt == null && c.DocenteId == docenteId &&
                c.PeriodoId == p.PeriodoId && c.AsignaturaId == p.AsignaturaId))
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .ToListAsync(cancellationToken);
        return planeaciones.Select(PlaneacionFlujoSupport.Resumen).ToList();
    }

    public async Task<PlaneacionEdicionDto> ObtenerDetalleAsync(Guid planeacionPublicId, long docenteId, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(docenteId, RolAutorizacion.Docente, cancellationToken);
        var planeacion = await PlaneacionFlujoSupport.BuscarDetalleAsync(context, planeacionPublicId, cancellationToken);
        await PlaneacionFlujoSupport.ExigirDocenteAsignadoAsync(context, planeacion, docenteId, cancellationToken);
        return PlaneacionFlujoSupport.Detalle(planeacion);
    }
}

public sealed class EdicionPlaneacionService(AppDbContext context, IAutorizacionService autorizacion, IPeriodoLifecycleService lifecycle) : IEdicionPlaneacionService
{
    public EdicionPlaneacionService(AppDbContext context, IAutorizacionService autorizacion) : this(context, autorizacion, PeriodoLifecycleService.ForContext(context)) { }
    public async Task<PlaneacionEdicionDto> ActualizarAsync(Guid planeacionPublicId, long docenteId, PlaneacionEdicionDto solicitud, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(docenteId, RolAutorizacion.Docente, cancellationToken);
        var planeacion = await PlaneacionFlujoSupport.BuscarDetalleAsync(context, planeacionPublicId, cancellationToken);
        await lifecycle.ExigirEditableAsync(planeacion.PeriodoId, cancellationToken);
        await PlaneacionFlujoSupport.ExigirDocenteAsignadoAsync(context, planeacion, docenteId, cancellationToken);
        if (planeacion.Estado is not (EstadoPlaneacion.Borrador or EstadoPlaneacion.CorreccionSolicitada or EstadoPlaneacion.EnProceso or EstadoPlaneacion.Reabierta))
            throw new AppException("Solo pueden editarse planeaciones en borrador, en proceso, con correcciones solicitadas o reabiertas.");

        await using var transaccion = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            PlaneacionFlujoSupport.ActualizarCaratula(planeacion, solicitud.Caratula, docenteId);
            PlaneacionFlujoSupport.SincronizarUnidades(planeacion, solicitud.Unidades, docenteId);
            PlaneacionFlujoSupport.SincronizarReferencias(planeacion, solicitud.Referencias, docenteId);
            planeacion.UltimaModificacionPorId = docenteId;
            planeacion.FechaUltimaModificacion = DateTime.UtcNow;
            planeacion.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
            return PlaneacionFlujoSupport.Detalle(planeacion);
        }
        catch
        {
            await transaccion.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

public sealed class AsignacionRevisorPlaneacionService(
    AppDbContext context,
    IAutorizacionService autorizacion,
    IPeriodoLifecycleService lifecycle,
    IFirebaseNotificacionService? firebaseNotificaciones = null,
    ILogger<AsignacionRevisorPlaneacionService>? logger = null) : IAsignacionRevisorPlaneacionService
{
    public AsignacionRevisorPlaneacionService(AppDbContext context, IAutorizacionService autorizacion)
        : this(context, autorizacion, PeriodoLifecycleService.ForContext(context), null, null) { }

    public AsignacionRevisorPlaneacionService(AppDbContext context, IAutorizacionService autorizacion, IPeriodoLifecycleService lifecycle)
        : this(context, autorizacion, lifecycle, null, null) { }

    public async Task<PlaneacionResumenDto> AsignarAsync(Guid planeacionPublicId, Guid revisorPublicId, long usuarioAutorizadoId, CancellationToken cancellationToken = default)
    {
        var planeacion = await PlaneacionFlujoSupport.BuscarDetalleAsync(context, planeacionPublicId, cancellationToken);
        await lifecycle.ExigirEditableAsync(planeacion.PeriodoId, cancellationToken);
        if (planeacion.Estado is EstadoPlaneacion.Aprobada or EstadoPlaneacion.Rechazada)
            throw new AppException("No se puede cambiar el revisor de una planeación resuelta.");
        await autorizacion.ExigirRolAsync(usuarioAutorizadoId, RolAutorizacion.Director, cancellationToken);

        var revisor = await context.Usuarios
            .FirstOrDefaultAsync(u => u.PublicId == revisorPublicId && u.Activo && u.DeletedAt == null, cancellationToken)
            ?? throw new AppException("El revisor indicado no existe o está inactivo.");
        if (!await autorizacion.HasRoleAsync(revisor.Id, RolAutorizacion.Revisor, cancellationToken))
            throw new AppException("El usuario seleccionado no tiene el rol de revisor para esta planeación.");

        planeacion.RevisorId = revisor.Id;
        planeacion.Revisor = revisor;
        planeacion.UltimaModificacionPorId = usuarioAutorizadoId;
        planeacion.FechaUltimaModificacion = DateTime.UtcNow;
        planeacion.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        await NotificarRevisorAsignadoAsync(revisor.Id, planeacion, cancellationToken);

        return PlaneacionFlujoSupport.Resumen(planeacion);
    }

    private async Task NotificarRevisorAsignadoAsync(long revisorId, PlaneacionDidactica planeacion, CancellationToken cancellationToken)
    {
        if (firebaseNotificaciones is null)
            return;

        try
        {
            var asignatura = PlaneacionFlujoSupport.ObtenerNombreAsignatura(planeacion);

            var titulo = "Nueva planeación asignada para revisión";
            var mensaje = $"Se te ha asignado como revisor de la planeación didáctica de {asignatura}.";

            var datos = new Dictionary<string, string>
            {
                ["tipo"] = "ASIGNACION_REVISOR",
                ["planeacionPublicId"] = planeacion.PublicId.ToString(),
                ["asignatura"] = asignatura
            };

            await firebaseNotificaciones.SendNotificationAsync(revisorId, titulo, mensaje, datos, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "No se pudo enviar la notificación push de asignación de revisor para el usuario {RevisorId}.", revisorId);
        }
    }
}

public sealed class PlaneacionesRevisorService(AppDbContext context, IAutorizacionService autorizacion) : IPlaneacionesRevisorService
{
    public async Task<IReadOnlyList<PlaneacionResumenDto>> ObtenerAsync(long revisorId, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(revisorId, RolAutorizacion.Revisor, cancellationToken);
        // Las planeaciones privadas del docente no se exponen al revisor hasta enviarse a revisión.
        var planeaciones = await PlaneacionFlujoSupport.QueryDetalle(context)
            .Where(p => p.Activo && p.DeletedAt == null && p.RevisorId == revisorId &&
                p.Estado != EstadoPlaneacion.Borrador && p.Estado != EstadoPlaneacion.EnProceso)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .ToListAsync(cancellationToken);
        return planeaciones.Select(PlaneacionFlujoSupport.Resumen).ToList();
    }

    public async Task<PlaneacionEdicionDto> ObtenerDetalleAsync(Guid planeacionPublicId, long revisorId, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(revisorId, RolAutorizacion.Revisor, cancellationToken);
        var planeacion = await PlaneacionFlujoSupport.BuscarDetalleAsync(context, planeacionPublicId, cancellationToken);
        PlaneacionFlujoSupport.ExigirRevisorAsignado(planeacion, revisorId);
        PlaneacionFlujoSupport.ExigirVisibleParaRevisor(planeacion);
        return PlaneacionFlujoSupport.Detalle(planeacion);
    }
}

public sealed class EstadoPlaneacionService(
    AppDbContext context,
    IAutorizacionService autorizacion,
    IPeriodoLifecycleService lifecycle,
    IFirebaseNotificacionService? firebaseNotificaciones = null,
    ILogger<EstadoPlaneacionService>? logger = null) : IEstadoPlaneacionService
{
    public EstadoPlaneacionService(AppDbContext context, IAutorizacionService autorizacion) : this(context, autorizacion, PeriodoLifecycleService.ForContext(context), null, null) { }
    public EstadoPlaneacionService(AppDbContext context, IAutorizacionService autorizacion, IPeriodoLifecycleService lifecycle) : this(context, autorizacion, lifecycle, null, null) { }

    public async Task<PlaneacionResumenDto> EnviarARevisionAsync(Guid planeacionPublicId, long docenteId, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(docenteId, RolAutorizacion.Docente, cancellationToken);
        var planeacion = await PlaneacionFlujoSupport.BuscarDetalleAsync(context, planeacionPublicId, cancellationToken);
        await lifecycle.ExigirEditableAsync(planeacion.PeriodoId, cancellationToken);
        await PlaneacionFlujoSupport.ExigirDocenteAsignadoAsync(context, planeacion, docenteId, cancellationToken);
        if (planeacion.Estado is not (EstadoPlaneacion.Borrador or EstadoPlaneacion.CorreccionSolicitada or EstadoPlaneacion.EnProceso or EstadoPlaneacion.Reabierta))
            throw new AppException("La planeación no está disponible para enviarse a revisión.");
        if (!planeacion.RevisorId.HasValue)
            throw new AppException("Debe asignarse un revisor antes de enviar la planeación a revisión.");

        var resumen = await CambiarAsync(planeacion, docenteId, EstadoPlaneacion.EnRevision, cancellationToken);

        await NotificarPlaneacionEnviadaARevisionAsync(planeacion.RevisorId.Value, planeacion, cancellationToken);

        return resumen;
    }

    public async Task<PlaneacionResumenDto> ResolverRevisionAsync(Guid planeacionPublicId, long revisorId, EstadoPlaneacion estado, CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(revisorId, RolAutorizacion.Revisor, cancellationToken);
        var planeacion = await PlaneacionFlujoSupport.BuscarDetalleAsync(context, planeacionPublicId, cancellationToken);
        await lifecycle.ExigirEditableAsync(planeacion.PeriodoId, cancellationToken);
        PlaneacionFlujoSupport.ExigirRevisorAsignado(planeacion, revisorId);

        if (estado == EstadoPlaneacion.Reabierta)
        {
            if (planeacion.Estado != EstadoPlaneacion.Aprobada)
                throw new AppException("Solo una planeación aprobada puede reabrirse.");
            var resumenReabierta = await CambiarAsync(planeacion, revisorId, estado, cancellationToken);
            await NotificarResolucionRevisionAsync(planeacion, estado, cancellationToken);
            return resumenReabierta;
        }

        if (estado is not (EstadoPlaneacion.Aprobada or EstadoPlaneacion.Rechazada or EstadoPlaneacion.CorreccionSolicitada))
            throw new AppException("El revisor solo puede aprobar, rechazar, solicitar correcciones o reabrir una planeación.");
        if (planeacion.Estado != EstadoPlaneacion.EnRevision)
            throw new AppException("Solo se pueden resolver planeaciones que están en revisión.");

        var resumen = await CambiarAsync(planeacion, revisorId, estado, cancellationToken);
        await NotificarResolucionRevisionAsync(planeacion, estado, cancellationToken);
        return resumen;
    }

    private async Task<PlaneacionResumenDto> CambiarAsync(PlaneacionDidactica planeacion, long usuarioId, EstadoPlaneacion estado, CancellationToken cancellationToken)
    {
        planeacion.Estado = estado;
        planeacion.UltimaModificacionPorId = usuarioId;
        planeacion.FechaUltimaModificacion = DateTime.UtcNow;
        planeacion.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return PlaneacionFlujoSupport.Resumen(planeacion);
    }

    private async Task NotificarPlaneacionEnviadaARevisionAsync(long revisorId, PlaneacionDidactica planeacion, CancellationToken cancellationToken)
    {
        if (firebaseNotificaciones is null)
            return;

        try
        {
            var asignatura = PlaneacionFlujoSupport.ObtenerNombreAsignatura(planeacion);
            var titulo = "Planeación enviada a revisión";
            var mensaje = $"La planeación didáctica de {asignatura} ha sido enviada para revisión.";

            var datos = new Dictionary<string, string>
            {
                ["tipo"] = "PLANEACION_ENVIADA_REVISION",
                ["planeacionPublicId"] = planeacion.PublicId.ToString(),
                ["asignatura"] = asignatura,
                ["estado"] = EstadoPlaneacion.EnRevision.ToString()
            };

            await firebaseNotificaciones.SendNotificationAsync(revisorId, titulo, mensaje, datos, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "No se pudo enviar la notificación push de envío a revisión para el revisor {RevisorId}.", revisorId);
        }
    }

    private async Task NotificarResolucionRevisionAsync(PlaneacionDidactica planeacion, EstadoPlaneacion estado, CancellationToken cancellationToken)
    {
        if (firebaseNotificaciones is null)
            return;

        try
        {
            var docentesIds = await context.CargasAcademicas
                .Where(c => c.Activo && c.DeletedAt == null && c.PeriodoId == planeacion.PeriodoId && c.AsignaturaId == planeacion.AsignaturaId)
                .Select(c => c.DocenteId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (docentesIds.Count == 0)
                return;

            var asignatura = PlaneacionFlujoSupport.ObtenerNombreAsignatura(planeacion);

            var (titulo, mensaje, tipo) = estado switch
            {
                EstadoPlaneacion.Aprobada => (
                    "Planeación aprobada",
                    $"La planeación didáctica de {asignatura} ha sido aprobada.",
                    "PLANEACION_APROBADA"),
                EstadoPlaneacion.Rechazada => (
                    "Planeación rechazada",
                    $"La planeación didáctica de {asignatura} ha sido rechazada.",
                    "PLANEACION_RECHAZADA"),
                EstadoPlaneacion.CorreccionSolicitada => (
                    "Correcciones solicitadas en planeación",
                    $"Se han solicitado correcciones para la planeación didáctica de {asignatura}.",
                    "PLANEACION_CORRECCION_SOLICITADA"),
                EstadoPlaneacion.Reabierta => (
                    "Planeación reabierta",
                    $"La planeación didáctica de {asignatura} ha sido reabierta para edición.",
                    "PLANEACION_REABIERTA"),
                _ => (
                    $"Actualización de planeación: {estado}",
                    $"La planeación didáctica de {asignatura} cambió al estado {estado}.",
                    "PLANEACION_CAMBIO_ESTADO")
            };

            var datos = new Dictionary<string, string>
            {
                ["tipo"] = tipo,
                ["planeacionPublicId"] = planeacion.PublicId.ToString(),
                ["asignatura"] = asignatura,
                ["estado"] = estado.ToString()
            };

            await firebaseNotificaciones.SendNotificationToUsersAsync(docentesIds, titulo, mensaje, datos, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "No se pudo enviar la notificación push de resolución de revisión ({Estado}) para la planeación {PlaneacionId}.", estado, planeacion.PublicId);
        }
    }
}

internal static class PlaneacionFlujoSupport
{
    internal static string ObtenerNombreAsignatura(PlaneacionDidactica planeacion) =>
        !string.IsNullOrWhiteSpace(planeacion.Asignatura?.Nombre)
            ? planeacion.Asignatura.Nombre
            : (!string.IsNullOrWhiteSpace(planeacion.Caratula?.NombreAsignatura)
                ? planeacion.Caratula.NombreAsignatura
                : "Asignatura");
    internal static IQueryable<PlaneacionDidactica> QueryDetalle(AppDbContext context) => context.PlaneacionesDidacticas
        .Include(p => p.Periodo)
        .Include(p => p.Asignatura)
        .Include(p => p.Revisor)
        .Include(p => p.Caratula)
        .Include(p => p.Unidades).ThenInclude(u => u.Temas)
        .Include(p => p.Unidades).ThenInclude(u => u.Evaluaciones)
        .Include(p => p.Unidades).ThenInclude(u => u.Secuencias)
        .Include(p => p.Unidades).ThenInclude(u => u.EtapasSecuencia).ThenInclude(e => e.Elementos).ThenInclude(s => s.Recursos)
        .Include(p => p.Referencias);

    internal static async Task<PlaneacionDidactica> BuscarDetalleAsync(AppDbContext context, Guid publicId, CancellationToken cancellationToken) =>
        await QueryDetalle(context).FirstOrDefaultAsync(p => p.PublicId == publicId && p.Activo && p.DeletedAt == null, cancellationToken)
        ?? throw new AppException("La planeación solicitada no existe.");

    internal static async Task ExigirDocenteAsignadoAsync(AppDbContext context, PlaneacionDidactica planeacion, long docenteId, CancellationToken cancellationToken)
    {
        var asignado = await context.CargasAcademicas.AnyAsync(c => c.Activo && c.DeletedAt == null && c.DocenteId == docenteId &&
            c.PeriodoId == planeacion.PeriodoId && c.AsignaturaId == planeacion.AsignaturaId, cancellationToken);
        if (!asignado) throw new ForbiddenException("No tiene asignada esta planeación como docente.");
    }

    internal static void ExigirRevisorAsignado(PlaneacionDidactica planeacion, long revisorId)
    {
        if (planeacion.RevisorId != revisorId) throw new ForbiddenException("No tiene asignada esta planeación como revisor.");
    }

    internal static PlaneacionResumenDto Resumen(PlaneacionDidactica p) => new()
    {
        PublicId = p.PublicId,
        Asignatura = p.Asignatura?.Nombre ?? p.Caratula?.NombreAsignatura ?? string.Empty,
        Periodo = p.Periodo?.Nombre ?? p.Caratula?.PeriodoEscolar ?? string.Empty,
        Grupos = p.Caratula?.Grupos ?? string.Empty,
        Docentes = p.Caratula?.Docentes ?? string.Empty,
        Estado = p.Estado,
        RevisorPublicId = p.Revisor?.PublicId,
        Revisor = p.Revisor is null ? null : NombreCompleto(p.Revisor),
        UltimaModificacion = p.FechaUltimaModificacion ?? p.UpdatedAt
    };

    internal static PlaneacionEdicionDto Detalle(PlaneacionDidactica p) => new()
    {
        PublicId = p.PublicId,
        Estado = p.Estado,
        Caratula = Caratula(p.Caratula),
        Unidades = p.Unidades.Where(u => u.Activo && u.DeletedAt == null).OrderBy(u => u.Orden).Select(Unidad).ToList(),
        Referencias = p.Referencias.Where(r => r.Activo && r.DeletedAt == null).OrderBy(r => r.Orden).Select(Referencia).ToList()
    };

    private static CaratulaPlaneacionEdicionDto Caratula(PlaneacionCaratula? c) => c is null ? new() : new()
    {
        ProgramaEducativo = c.ProgramaEducativo, Cuatrimestre = c.Cuatrimestre, NombreAsignatura = c.NombreAsignatura,
        Docentes = c.Docentes, PeriodoEscolar = c.PeriodoEscolar, Grupos = c.Grupos, PropositoAsignatura = c.PropositoAsignatura,
        CompetenciaAsignatura = c.CompetenciaAsignatura, TipoCompetencia = c.TipoCompetencia, Creditos = c.Creditos,
        Modalidad = c.Modalidad, HorasSaber = c.HorasSaber, HorasSaberHacer = c.HorasSaberHacer,
        HorasTotales = c.HorasTotales, HorasSemana = c.HorasSemana
    };

    private static UnidadPlaneacionEdicionDto Unidad(PlaneacionUnidad u) => new()
    {
        PublicId = u.PublicId, NumeroUnidad = u.NumeroUnidad, NombreUnidad = u.NombreUnidad, PropositoEsperado = u.PropositoEsperado,
        HorasSaber = u.HorasSaber, HorasSaberHacer = u.HorasSaberHacer, HorasTotales = u.HorasTotales,
        PorcentajeUnidad = u.PorcentajeUnidad, Orden = u.Orden,
        Temas = u.Temas.Where(t => t.Activo && t.DeletedAt == null).OrderBy(t => t.Orden).Select(Tema).ToList(),
        Evaluaciones = u.Evaluaciones.Where(e => e.Activo && e.DeletedAt == null).OrderBy(e => e.Orden).Select(Evaluacion).ToList(),
        Apertura = SecuenciasDeFase(u, FaseSecuencia.Apertura),
        Desarrollo = SecuenciasDeFase(u, FaseSecuencia.Desarrollo),
        Cierre = SecuenciasDeFase(u, FaseSecuencia.Cierre)
    };

    private static TemaPlaneacionEdicionDto Tema(PlaneacionTema t) => new() { PublicId = t.PublicId, Tema = t.Tema, SaberConceptual = t.SaberConceptual, SaberHacer = t.SaberHacer, SaberSer = t.SaberSer, Orden = t.Orden };
    private static EvaluacionPlaneacionEdicionDto Evaluacion(PlaneacionEvaluacion e) => new() { PublicId = e.PublicId, PeriodoSemanas = e.PeriodoSemanas, ResultadoAprendizaje = e.ResultadoAprendizaje, EvidenciaAprendizaje = e.EvidenciaAprendizaje, Fase = e.Fase, TipoEvaluacion = e.TipoEvaluacion, AgenteEvaluador = e.AgenteEvaluador, Ponderacion = e.Ponderacion, InstrumentoEvaluacion = e.InstrumentoEvaluacion, Orden = e.Orden };
    private static List<SecuenciaPlaneacionEdicionDto> SecuenciasDeFase(PlaneacionUnidad unidad, FaseSecuencia fase) => unidad.Secuencias
        .Where(s => s.Activo && s.DeletedAt == null && s.Fase == fase)
        .OrderBy(s => s.Orden).Select(Secuencia).ToList();
    private static SecuenciaPlaneacionEdicionDto Secuencia(PlaneacionSecuencia s) => new()
    {
        PublicId = s.PublicId, Fase = s.Fase, MetodoTecnica = s.MetodoTecnica, Estrategia = s.Estrategia,
        ActividadDocente = s.ActividadDocente, ActividadEstudiante = s.ActividadEstudiante,
        EvidenciaAprendizaje = s.EvidenciaAprendizaje, MediosMateriales = s.MediosMateriales, Orden = s.Orden,
        Recursos = s.Recursos.Where(r => r.Activo && r.DeletedAt == null).OrderBy(r => r.Orden)
            .Select(r => new RecursoSecuenciaPlaneacionEdicionDto { PublicId = r.PublicId, Nombre = r.Nombre, Orden = r.Orden }).ToList()
    };
    private static ReferenciaPlaneacionEdicionDto Referencia(PlaneacionReferencia r) => new() { PublicId = r.PublicId, ReferenciaAPA = r.ReferenciaAPA, Orden = r.Orden };
    private static string NombreCompleto(Usuario u) => string.Join(" ", new[] { u.Nombre, u.ApellidoPaterno, u.ApellidoMaterno }.Where(x => !string.IsNullOrWhiteSpace(x)));

    internal static void ActualizarCaratula(PlaneacionDidactica p, CaratulaPlaneacionEdicionDto d, long usuarioId)
    {
        var c = p.Caratula ??= new PlaneacionCaratula { PlaneacionDidacticaId = p.Id };
        c.ProgramaEducativo = d.ProgramaEducativo; c.Cuatrimestre = d.Cuatrimestre; c.NombreAsignatura = d.NombreAsignatura;
        c.Docentes = d.Docentes; c.PeriodoEscolar = d.PeriodoEscolar; c.Grupos = d.Grupos; c.PropositoAsignatura = d.PropositoAsignatura;
        c.CompetenciaAsignatura = d.CompetenciaAsignatura; c.TipoCompetencia = d.TipoCompetencia; c.Creditos = d.Creditos;
        c.Modalidad = d.Modalidad; c.HorasSaber = d.HorasSaber; c.HorasSaberHacer = d.HorasSaberHacer; c.HorasTotales = d.HorasTotales; c.HorasSemana = d.HorasSemana;
        c.UltimaModificacionPorId = usuarioId; c.UpdatedAt = DateTime.UtcNow; c.Version++;
    }

    internal static void SincronizarUnidades(PlaneacionDidactica p, IReadOnlyCollection<UnidadPlaneacionEdicionDto> datos, long usuarioId)
    {
        ValidarIds(datos.Select(x => x.PublicId), "unidades");
        var existentes = p.Unidades.Where(x => x.Activo && x.DeletedAt == null).ToDictionary(x => x.PublicId);
        var idsRecibidos = datos.Where(x => x.PublicId.HasValue).Select(x => x.PublicId!.Value).ToHashSet();
        foreach (var x in existentes.Values.Where(x => !idsRecibidos.Contains(x.PublicId))) Desactivar(x);
        foreach (var d in datos)
        {
            var u = d.PublicId.HasValue
                ? existentes.GetValueOrDefault(d.PublicId.Value) ?? throw new AppException("Una unidad no pertenece a la planeación.")
                : AgregarUnidad(p);
            u.NumeroUnidad = d.NumeroUnidad; u.NombreUnidad = d.NombreUnidad; u.PropositoEsperado = d.PropositoEsperado;
            u.HorasSaber = d.HorasSaber; u.HorasSaberHacer = d.HorasSaberHacer; u.HorasTotales = d.HorasTotales; u.PorcentajeUnidad = d.PorcentajeUnidad; u.Orden = d.Orden;
            u.UltimaModificacionPorId = usuarioId; u.FechaUltimaModificacion = DateTime.UtcNow; u.UpdatedAt = DateTime.UtcNow;
            SincronizarTemas(u, d.Temas, usuarioId); SincronizarEvaluaciones(u, d.Evaluaciones, usuarioId); SincronizarSecuencias(u, d, usuarioId);
        }
    }

    internal static void SincronizarReferencias(PlaneacionDidactica p, IReadOnlyCollection<ReferenciaPlaneacionEdicionDto> datos, long usuarioId)
    {
        ValidarIds(datos.Select(x => x.PublicId), "referencias");
        var existentes = p.Referencias.Where(x => x.Activo && x.DeletedAt == null).ToDictionary(x => x.PublicId);
        var ids = datos.Where(x => x.PublicId.HasValue).Select(x => x.PublicId!.Value).ToHashSet();
        foreach (var x in existentes.Values.Where(x => !ids.Contains(x.PublicId))) Desactivar(x);
        foreach (var d in datos)
        {
            var r = d.PublicId.HasValue ? existentes.GetValueOrDefault(d.PublicId.Value) ?? throw new AppException("Una referencia no pertenece a la planeación.") : AgregarReferencia(p);
            r.ReferenciaAPA = d.ReferenciaAPA; r.Orden = d.Orden; r.UltimaModificacionPorId = usuarioId; r.FechaUltimaModificacion = DateTime.UtcNow; r.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static void SincronizarTemas(PlaneacionUnidad u, IReadOnlyCollection<TemaPlaneacionEdicionDto> datos, long usuarioId)
    {
        ValidarIds(datos.Select(x => x.PublicId), "temas"); var e = u.Temas.Where(x => x.Activo && x.DeletedAt == null).ToDictionary(x => x.PublicId); var ids = datos.Where(x => x.PublicId.HasValue).Select(x => x.PublicId!.Value).ToHashSet();
        foreach (var x in e.Values.Where(x => !ids.Contains(x.PublicId))) Desactivar(x);
        foreach (var d in datos) { var x = d.PublicId.HasValue ? e.GetValueOrDefault(d.PublicId.Value) ?? throw new AppException("Un tema no pertenece a la unidad.") : AgregarTema(u); x.Tema = d.Tema; x.SaberConceptual = d.SaberConceptual; x.SaberHacer = d.SaberHacer; x.SaberSer = d.SaberSer; x.Orden = d.Orden; Auditar(x, usuarioId); }
    }

    private static void SincronizarEvaluaciones(PlaneacionUnidad u, IReadOnlyCollection<EvaluacionPlaneacionEdicionDto> datos, long usuarioId)
    {
        ValidarIds(datos.Select(x => x.PublicId), "evaluaciones"); var e = u.Evaluaciones.Where(x => x.Activo && x.DeletedAt == null).ToDictionary(x => x.PublicId); var ids = datos.Where(x => x.PublicId.HasValue).Select(x => x.PublicId!.Value).ToHashSet();
        foreach (var x in e.Values.Where(x => !ids.Contains(x.PublicId))) Desactivar(x);
        foreach (var d in datos) { var x = d.PublicId.HasValue ? e.GetValueOrDefault(d.PublicId.Value) ?? throw new AppException("Una evaluación no pertenece a la unidad.") : AgregarEvaluacion(u); x.PeriodoSemanas = d.PeriodoSemanas; x.ResultadoAprendizaje = d.ResultadoAprendizaje; x.EvidenciaAprendizaje = d.EvidenciaAprendizaje; x.Fase = d.Fase; x.TipoEvaluacion = d.TipoEvaluacion; x.AgenteEvaluador = d.AgenteEvaluador; x.Ponderacion = d.Ponderacion; x.InstrumentoEvaluacion = d.InstrumentoEvaluacion; x.Orden = d.Orden; Auditar(x, usuarioId); }
    }

    private static void SincronizarSecuencias(PlaneacionUnidad unidad, UnidadPlaneacionEdicionDto datos, long usuarioId)
    {
        AsegurarEtapas(unidad);
        var agrupadas = new Dictionary<FaseSecuencia, IReadOnlyCollection<SecuenciaPlaneacionEdicionDto>>();
        if (datos.Secuencias is not null)
        {
            if (datos.Apertura is not null || datos.Desarrollo is not null || datos.Cierre is not null)
                throw new AppException("Envíe las secuencias agrupadas por etapa o la lista heredada, no ambas.");
            foreach (var grupo in datos.Secuencias.GroupBy(s => s.Fase ?? throw new AppException("Una secuencia heredada debe indicar su fase.")))
                agrupadas[grupo.Key] = grupo.ToList();
            foreach (var fase in Fases) agrupadas.TryAdd(fase, []);
        }
        else
        {
            if (datos.Apertura is not null) agrupadas[FaseSecuencia.Apertura] = datos.Apertura;
            if (datos.Desarrollo is not null) agrupadas[FaseSecuencia.Desarrollo] = datos.Desarrollo;
            if (datos.Cierre is not null) agrupadas[FaseSecuencia.Cierre] = datos.Cierre;
        }

        foreach (var (fase, elementos) in agrupadas)
            SincronizarEtapa(unidad, fase, elementos, usuarioId);
    }

    private static readonly FaseSecuencia[] Fases = [FaseSecuencia.Apertura, FaseSecuencia.Desarrollo, FaseSecuencia.Cierre];

    internal static void AsegurarEtapas(PlaneacionUnidad unidad)
    {
        foreach (var fase in Fases)
        {
            var etapa = unidad.EtapasSecuencia.FirstOrDefault(e => e.Fase == fase);
            if (etapa is null)
                unidad.EtapasSecuencia.Add(new PlaneacionEtapaSecuencia { Fase = fase });
            else if (!etapa.Activo || etapa.DeletedAt is not null)
            {
                etapa.Activo = true;
                etapa.DeletedAt = null;
            }
        }
    }

    private static void SincronizarEtapa(PlaneacionUnidad unidad, FaseSecuencia fase, IReadOnlyCollection<SecuenciaPlaneacionEdicionDto> datos, long usuarioId)
    {
        ValidarIds(datos.Select(x => x.PublicId), $"secuencias de {fase}");
        var etapa = unidad.EtapasSecuencia.Single(e => e.Fase == fase && e.Activo && e.DeletedAt == null);
        var existentes = etapa.Elementos.Where(x => x.Activo && x.DeletedAt == null).ToDictionary(x => x.PublicId);
        var ids = datos.Where(x => x.PublicId.HasValue).Select(x => x.PublicId!.Value).ToHashSet();
        foreach (var existente in existentes.Values.Where(x => !ids.Contains(x.PublicId))) Desactivar(existente);

        foreach (var dato in datos)
        {
            if (dato.Fase.HasValue && dato.Fase.Value != fase)
                throw new AppException("La fase de un elemento no coincide con la etapa donde se capturó.");
            if (dato.MetodoTecnica.HasValue && !MetodoPermitidoEnFase(dato.MetodoTecnica.Value, fase))
                throw new AppException($"El método o técnica {dato.MetodoTecnica} no está permitido en {fase}.");

            var elemento = dato.PublicId.HasValue
                ? existentes.GetValueOrDefault(dato.PublicId.Value) ?? throw new AppException("Una secuencia no pertenece a esta etapa de la unidad.")
                : AgregarSecuencia(unidad, etapa, fase);
            if (!dato.MetodoTecnica.HasValue && !dato.Estrategia.HasValue && !elemento.MetodoTecnica.HasValue)
                throw new AppException("Cada elemento de secuencia requiere un método o técnica.");

            elemento.Fase = fase;
            elemento.EtapaSecuencia = etapa;
            elemento.MetodoTecnica = dato.MetodoTecnica ?? elemento.MetodoTecnica;
            elemento.Estrategia = dato.Estrategia ?? elemento.Estrategia;
            elemento.ActividadDocente = dato.ActividadDocente;
            elemento.ActividadEstudiante = dato.ActividadEstudiante;
            elemento.EvidenciaAprendizaje = dato.EvidenciaAprendizaje;
            elemento.MediosMateriales = dato.MediosMateriales;
            elemento.Orden = dato.Orden;
            SincronizarRecursos(elemento, dato.Recursos);
            Auditar(elemento, usuarioId);
        }
    }

    private static bool MetodoPermitidoEnFase(MetodoTecnicaEnsenanzaAprendizaje metodo, FaseSecuencia fase) => fase switch
    {
        FaseSecuencia.Apertura => metodo is MetodoTecnicaEnsenanzaAprendizaje.WebQuest or MetodoTecnicaEnsenanzaAprendizaje.TecnicaExpositiva or MetodoTecnicaEnsenanzaAprendizaje.Conceptual or MetodoTecnicaEnsenanzaAprendizaje.LluviaDeIdeas or MetodoTecnicaEnsenanzaAprendizaje.CuadroSinoptico or MetodoTecnicaEnsenanzaAprendizaje.MapaMental or MetodoTecnicaEnsenanzaAprendizaje.MapaConceptual or MetodoTecnicaEnsenanzaAprendizaje.Investigacion or MetodoTecnicaEnsenanzaAprendizaje.LecturaComentada,
        FaseSecuencia.Desarrollo => metodo is MetodoTecnicaEnsenanzaAprendizaje.Taller or MetodoTecnicaEnsenanzaAprendizaje.Ensayo or MetodoTecnicaEnsenanzaAprendizaje.EstudioDeCaso or MetodoTecnicaEnsenanzaAprendizaje.Debate or MetodoTecnicaEnsenanzaAprendizaje.Foro or MetodoTecnicaEnsenanzaAprendizaje.Panel or MetodoTecnicaEnsenanzaAprendizaje.Seminario or MetodoTecnicaEnsenanzaAprendizaje.MesaRedonda or MetodoTecnicaEnsenanzaAprendizaje.ProyectoDeInvestigacion or MetodoTecnicaEnsenanzaAprendizaje.AprendizajeBasadoEnProblemas or MetodoTecnicaEnsenanzaAprendizaje.AprendizajePorProyectos or MetodoTecnicaEnsenanzaAprendizaje.AprendizajeCooperativo or MetodoTecnicaEnsenanzaAprendizaje.PracticaGuiada or MetodoTecnicaEnsenanzaAprendizaje.PracticaDeLaboratorio,
        FaseSecuencia.Cierre => metodo is MetodoTecnicaEnsenanzaAprendizaje.AnalisisDeDesempeno or MetodoTecnicaEnsenanzaAprendizaje.CuestionarioReflexion or MetodoTecnicaEnsenanzaAprendizaje.Ensayo or MetodoTecnicaEnsenanzaAprendizaje.MapaMental or MetodoTecnicaEnsenanzaAprendizaje.MapaConceptual or MetodoTecnicaEnsenanzaAprendizaje.Debate or MetodoTecnicaEnsenanzaAprendizaje.Foro or MetodoTecnicaEnsenanzaAprendizaje.Panel or MetodoTecnicaEnsenanzaAprendizaje.Seminario or MetodoTecnicaEnsenanzaAprendizaje.MesaRedonda,
        _ => false
    };

    private static void SincronizarRecursos(PlaneacionSecuencia elemento, IReadOnlyCollection<RecursoSecuenciaPlaneacionEdicionDto>? datos)
    {
        if (datos is null) return;
        ValidarIds(datos.Select(x => x.PublicId), "recursos");
        var existentes = elemento.Recursos.Where(x => x.Activo && x.DeletedAt == null).ToDictionary(x => x.PublicId);
        var ids = datos.Where(x => x.PublicId.HasValue).Select(x => x.PublicId!.Value).ToHashSet();
        foreach (var existente in existentes.Values.Where(x => !ids.Contains(x.PublicId))) Desactivar(existente);
        foreach (var dato in datos)
        {
            var recurso = dato.PublicId.HasValue
                ? existentes.GetValueOrDefault(dato.PublicId.Value) ?? throw new AppException("Un recurso no pertenece al elemento de secuencia.")
                : new PlaneacionSecuenciaRecurso();
            if (!dato.PublicId.HasValue) elemento.Recursos.Add(recurso);
            recurso.Nombre = dato.Nombre;
            recurso.Orden = dato.Orden;
            recurso.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static PlaneacionUnidad AgregarUnidad(PlaneacionDidactica p) { var x = new PlaneacionUnidad(); p.Unidades.Add(x); return x; }
    private static PlaneacionTema AgregarTema(PlaneacionUnidad u) { var x = new PlaneacionTema(); u.Temas.Add(x); return x; }
    private static PlaneacionEvaluacion AgregarEvaluacion(PlaneacionUnidad u) { var x = new PlaneacionEvaluacion(); u.Evaluaciones.Add(x); return x; }
    private static PlaneacionSecuencia AgregarSecuencia(PlaneacionUnidad u, PlaneacionEtapaSecuencia etapa, FaseSecuencia fase) { var x = new PlaneacionSecuencia { EtapaSecuencia = etapa, Fase = fase }; u.Secuencias.Add(x); return x; }
    private static PlaneacionReferencia AgregarReferencia(PlaneacionDidactica p) { var x = new PlaneacionReferencia(); p.Referencias.Add(x); return x; }
    private static void Auditar(PlaneacionTema x, long id) { x.UltimaModificacionPorId = id; x.FechaUltimaModificacion = DateTime.UtcNow; x.UpdatedAt = DateTime.UtcNow; }
    private static void Auditar(PlaneacionEvaluacion x, long id) { x.UltimaModificacionPorId = id; x.FechaUltimaModificacion = DateTime.UtcNow; x.UpdatedAt = DateTime.UtcNow; }
    private static void Auditar(PlaneacionSecuencia x, long id) { x.UltimaModificacionPorId = id; x.FechaUltimaModificacion = DateTime.UtcNow; x.UpdatedAt = DateTime.UtcNow; }
    private static void Desactivar(BaseEntity x) { x.Activo = false; x.DeletedAt = DateTime.UtcNow; x.UpdatedAt = DateTime.UtcNow; }
    private static void ValidarIds(IEnumerable<Guid?> ids, string seccion) { var recibidos = ids.Where(x => x.HasValue).Select(x => x!.Value).ToList(); if (recibidos.Count != recibidos.Distinct().Count()) throw new AppException($"Hay identificadores duplicados en {seccion}."); }

    internal static void ExigirVisibleParaRevisor(PlaneacionDidactica planeacion)
    {
        if (planeacion.Estado is EstadoPlaneacion.Borrador or EstadoPlaneacion.EnProceso)
            throw new ForbiddenException("La planeación aún no ha sido enviada a revisión.");
    }
}
