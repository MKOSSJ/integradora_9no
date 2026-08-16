using Microsoft.AspNetCore.Mvc;
using Plandi.Dto;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _notificacionService;
    private readonly ILogger<NotificacionesController> _logger;

    public NotificacionesController(INotificacionService notificacionService, ILogger<NotificacionesController> logger)
    {
        _notificacionService = notificacionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] long usuarioId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanioPagina = 20)
    {
        try
        {
            if (usuarioId <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El usuarioId es requerido"
                });
            }

            if (pagina < 1)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "La página debe ser mayor o igual a 1"
                });
            }

            if (tamanioPagina < 1)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El tamaño de página debe ser mayor o igual a 1"
                });
            }

            var notificaciones = await _notificacionService.ListarAsync(usuarioId, pagina, tamanioPagina);

            return Ok(new ApiResponse<NotificacionesPaginadasDto>
            {
                Success = true,
                Data = notificaciones,
                Message = "Notificaciones obtenidas correctamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones del usuario {UsuarioId}", usuarioId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al obtener las notificaciones"
            });
        }
    }

    [HttpPatch("{id}/leer")]
    public async Task<IActionResult> MarcarComoLeida(long id, [FromQuery] long usuarioId)
    {
        try
        {
            if (usuarioId <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El usuarioId es requerido"
                });
            }

            var resultado = await _notificacionService.MarcarComoLeidaAsync(id, usuarioId);

            if (!resultado.Exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Notificación {id} no encontrada"
                });
            }

            if (!resultado.Authorized)
            {
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = "No tienes permiso para marcar esta notificación como leída"
                });
            }

            return Ok(new ApiResponse<NotificacionDto>
            {
                Success = true,
                Data = resultado.Notificacion,
                Message = "Notificación marcada como leída"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificación {NotificacionId} como leída", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al marcar la notificación como leída"
            });
        }
    }
}
