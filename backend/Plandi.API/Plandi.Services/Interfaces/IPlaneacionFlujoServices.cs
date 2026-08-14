using Plandi.Dto.Catalogos;
using Plandi.Dto.Enums;

namespace Plandi.Services.Interfaces;

public interface IMisPlaneacionesDocenteService
{
    Task<IReadOnlyList<PlaneacionResumenDto>> ObtenerAsync(long docenteId, CancellationToken cancellationToken = default);
    Task<PlaneacionEdicionDto> ObtenerDetalleAsync(Guid planeacionPublicId, long docenteId, CancellationToken cancellationToken = default);
}

public interface IEdicionPlaneacionService
{
    Task<PlaneacionEdicionDto> ActualizarAsync(Guid planeacionPublicId, long docenteId, PlaneacionEdicionDto solicitud, CancellationToken cancellationToken = default);
}

public interface IAsignacionRevisorPlaneacionService
{
    Task<PlaneacionResumenDto> AsignarAsync(Guid planeacionPublicId, Guid revisorPublicId, long usuarioAutorizadoId, CancellationToken cancellationToken = default);
}

public interface IPlaneacionesRevisorService
{
    Task<IReadOnlyList<PlaneacionResumenDto>> ObtenerAsync(long revisorId, CancellationToken cancellationToken = default);
    Task<PlaneacionEdicionDto> ObtenerDetalleAsync(Guid planeacionPublicId, long revisorId, CancellationToken cancellationToken = default);
}

public interface IEstadoPlaneacionService
{
    Task<PlaneacionResumenDto> EnviarARevisionAsync(Guid planeacionPublicId, long docenteId, CancellationToken cancellationToken = default);
    Task<PlaneacionResumenDto> ResolverRevisionAsync(Guid planeacionPublicId, long revisorId, EstadoPlaneacion estado, CancellationToken cancellationToken = default);
}
