using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/planeaciones/{planeacionPublicId:guid}/comentarios-correccion")]
public sealed class ComentariosCorreccionController(
    IComentariosCorreccionService comentarios,
    IAutorizacionService autorizacion) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ComentarioCorreccionDto>>> Crear(
        Guid planeacionPublicId,
        [FromBody] CrearComentarioCorreccionDto solicitud,
        CancellationToken cancellationToken) =>
        Ok(ApiResponse<ComentarioCorreccionDto>.Ok(
            await comentarios.CrearAsync(planeacionPublicId, solicitud, UsuarioId, cancellationToken),
            "Comentario creado."));

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ComentariosCorreccionDto>>> Listar(
        Guid planeacionPublicId,
        CancellationToken cancellationToken) =>
        Ok(ApiResponse<ComentariosCorreccionDto>.Ok(
            await comentarios.ListarAsync(planeacionPublicId, UsuarioId, cancellationToken)));

    private long UsuarioId => autorizacion.ObtenerUsuarioId(User);
}
