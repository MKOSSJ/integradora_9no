using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class RelojAcademico : IRelojAcademico
{
    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _timeZone;

    public RelojAcademico(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _timeZone = ResolveTimeZone();
    }

    public DateTime AhoraUtc => _timeProvider.GetUtcNow().UtcDateTime;
    public DateTime AhoraLocal => TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), _timeZone).DateTime;

    private static TimeZoneInfo ResolveTimeZone()
    {
        foreach (var id in new[] { "America/Mexico_City", "Central Standard Time (Mexico)" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
        }
        throw new InvalidOperationException("No fue posible cargar la zona horaria de Ciudad de México.");
    }
}

public sealed class PeriodoLifecycleService(AppDbContext context, IRelojAcademico reloj) : IPeriodoLifecycleService
{
    public const string MensajePeriodoCerrado = "El periodo se encuentra cerrado y la información ya no puede modificarse.";
    public const string MensajePeriodoNoVigente = "El periodo todavía no se encuentra vigente y la información no puede modificarse.";

    internal static IPeriodoLifecycleService ForContext(AppDbContext context) => new PeriodoLifecycleService(context, new RelojAcademico(TimeProvider.System));

    public EstadoPeriodo ObtenerEstadoEfectivo(Periodo periodo)
    {
        if (periodo.Estado == EstadoPeriodo.Cerrado || EstaVencido(periodo)) return EstadoPeriodo.Cerrado;
        if (periodo.FechaInicio != default && periodo.FechaInicio.Date > reloj.AhoraLocal.Date) return EstadoPeriodo.Programado;
        return EstadoPeriodo.Activo;
    }

    public bool PermiteModificaciones(Periodo periodo) =>
        periodo.Activo && periodo.DeletedAt is null && ObtenerEstadoEfectivo(periodo) == EstadoPeriodo.Activo;

    public async Task ExigirEditableAsync(long periodoId, CancellationToken cancellationToken = default)
    {
        var periodo = await context.Periodos.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == periodoId && x.DeletedAt == null, cancellationToken)
            ?? throw new NotFoundException("El periodo especificado no existe.");

        var estado = ObtenerEstadoEfectivo(periodo);
        if (estado == EstadoPeriodo.Cerrado) throw new ConflictException(MensajePeriodoCerrado);
        if (!periodo.Activo || estado == EstadoPeriodo.Programado) throw new ConflictException(MensajePeriodoNoVigente);
    }

    public async Task<int> ActualizarEstadosAsync(CancellationToken cancellationToken = default)
    {
        var periodos = await context.Periodos
            .Where(x => x.Activo && x.DeletedAt == null && x.Estado != EstadoPeriodo.Cerrado)
            .ToListAsync(cancellationToken);
        var cambios = 0;
        foreach (var periodo in periodos)
        {
            var efectivo = ObtenerEstadoEfectivo(periodo);
            if (periodo.Estado == efectivo) continue;
            periodo.Estado = efectivo;
            periodo.UpdatedAt = reloj.AhoraUtc;
            if (efectivo == EstadoPeriodo.Cerrado) periodo.FechaCierre ??= reloj.AhoraUtc;
            cambios++;
        }
        if (cambios > 0) await context.SaveChangesAsync(cancellationToken);
        return cambios;
    }

    public async Task CerrarAsync(long periodoId, long actorId, CancellationToken cancellationToken = default)
    {
        var periodo = await context.Periodos.SingleOrDefaultAsync(x => x.Id == periodoId && x.DeletedAt == null, cancellationToken)
            ?? throw new NotFoundException("El periodo especificado no existe.");
        if (periodo.Estado == EstadoPeriodo.Cerrado) return;
        periodo.Estado = EstadoPeriodo.Cerrado;
        periodo.FechaCierre = reloj.AhoraUtc;
        periodo.UpdatedAt = reloj.AhoraUtc;
        periodo.UpdatedBy = actorId;
        await context.SaveChangesAsync(cancellationToken);
    }

    private bool EstaVencido(Periodo periodo) =>
        periodo.FechaFin != default && periodo.FechaFin.Date < reloj.AhoraLocal.Date;
}
