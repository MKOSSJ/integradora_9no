using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces;

public interface IComentariosCorreccionService
{
    Task<ComentarioCorreccionDto> CrearAsync(
        Guid planeacionPublicId,
        CrearComentarioCorreccionDto solicitud,
        long usuarioId,
        CancellationToken cancellationToken = default);

    Task<ComentariosCorreccionDto> ListarAsync(
        Guid planeacionPublicId,
        long usuarioId,
        CancellationToken cancellationToken = default);
}
