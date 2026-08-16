using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;

namespace Plandi.Services.Interfaces;

public interface IRepositorioService
{
    Task<PagedResult<RepositorioPlaneacionDto>> BuscarAsync(RepositorioPlaneacionesFiltroDto filtro, long usuarioId, CancellationToken ct = default);
    Task<RepositorioPlaneacionDto> ObtenerAsync(Guid planeacionPublicId, long usuarioId, CancellationToken ct = default);
    Task<IReadOnlyList<RepositorioArchivoDto>> ArchivosAsync(Guid planeacionPublicId, long usuarioId, CancellationToken ct = default);
    Task<ArchivoContenido> DescargarAsync(Guid planeacionPublicId, string tipo, long usuarioId, CancellationToken ct = default);
}
