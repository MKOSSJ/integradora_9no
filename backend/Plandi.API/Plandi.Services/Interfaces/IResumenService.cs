using Plandi.Dto.Resumen;

namespace Plandi.Services.Interfaces;

public interface IResumenService
{
    Task<ResumenDashboardDto> ObtenerDashboardAsync(CancellationToken cancellationToken = default);
    Task<ResumenUsuariosDto> ObtenerUsuariosAsync(CancellationToken cancellationToken = default);
    Task<ResumenCarrerasDto> ObtenerCarrerasAsync(CancellationToken cancellationToken = default);
    Task<ResumenAsignaturasDto> ObtenerAsignaturasAsync(CancellationToken cancellationToken = default);
    Task<ResumenCiclosEscolaresDto> ObtenerCiclosEscolaresAsync(CancellationToken cancellationToken = default);
    Task<ResumenPeriodosDto> ObtenerPeriodosAsync(CancellationToken cancellationToken = default);
    Task<ResumenGruposDto> ObtenerGruposAsync(CancellationToken cancellationToken = default);
    Task<ResumenAsignacionAcademicaDto> ObtenerAsignacionAcademicaAsync(CancellationToken cancellationToken = default);
    Task<ResumenSeguimientoPlaneacionesDto> ObtenerSeguimientoPlaneacionesAsync(CancellationToken cancellationToken = default);
    Task<ResumenDashboardDocenteDto> ObtenerDashboardDocenteAsync(long docenteId, CancellationToken cancellationToken = default);
    Task<ResumenPlaneacionesDocenteDto> ObtenerPlaneacionesDocenteAsync(long docenteId, CancellationToken cancellationToken = default);
    Task<ResumenDashboardRevisorDto> ObtenerDashboardRevisorAsync(long revisorId, CancellationToken cancellationToken = default);
    Task<ResumenValidacionDto> ObtenerValidacionAsync(long revisorId, CancellationToken cancellationToken = default);
}
