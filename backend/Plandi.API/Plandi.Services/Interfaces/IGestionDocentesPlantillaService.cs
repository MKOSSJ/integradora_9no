using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces;

public interface IGestionDocentesPlantillaService
{
    Task<CredencialesDocenteDto> CompletarCredencialesAsync(
        Guid usuarioPublicId,
        CompletarCredencialesDocenteDto solicitud,
        long usuarioAutorizadoId,
        CancellationToken cancellationToken = default);
}
