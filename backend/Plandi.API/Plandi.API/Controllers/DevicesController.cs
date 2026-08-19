using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/devices")]
public sealed class DevicesController(
    IFirebaseNotificacionService firebaseNotificacionService,
    IAutorizacionService autorizacionService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<object>>> Register(
        [FromBody] RegistrarDispositivoDto dto,
        CancellationToken cancellationToken)
    {
        await firebaseNotificacionService.RegistrarTokenAsync(UsuarioId, dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Dispositivo registrado exitosamente."));
    }

    [HttpPost("unregister")]
    public async Task<ActionResult<ApiResponse<object>>> Unregister(
        [FromBody] string fcmToken,
        CancellationToken cancellationToken)
    {
        await firebaseNotificacionService.DesactivarTokenAsync(fcmToken, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Dispositivo desregistrado exitosamente."));
    }

    [HttpPost("saludar/{usuarioId:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> Saludar(
        [FromRoute] long usuarioId,
        CancellationToken cancellationToken)
    {
        var enviado = await firebaseNotificacionService.SaludarUsuarioAsync(usuarioId, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(
            enviado,
            enviado
                ? "Notificación de saludo enviada exitosamente."
                : "No se pudo enviar el saludo (el usuario no cuenta con dispositivos activos o falló Firebase)."));
    }

    private long UsuarioId => autorizacionService.ObtenerUsuarioId(User);
}
