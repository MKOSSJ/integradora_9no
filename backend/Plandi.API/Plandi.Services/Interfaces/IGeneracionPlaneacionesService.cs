using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces;

public interface IGeneracionPlaneacionesService
{
    Task<GeneracionPlaneacionesResultadoDto> GenerarAsync(CancellationToken cancellationToken = default);
}
