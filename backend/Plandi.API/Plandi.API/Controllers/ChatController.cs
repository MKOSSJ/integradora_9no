using Microsoft.AspNetCore.Mvc;
using Plandi.Dto;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el historial paginado de mensajes de un chat.
    /// </summary>
    [HttpGet("{chatId}/mensajes")]
    public async Task<IActionResult> GetMensajes(
        long chatId,
        [FromQuery] long usuarioId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanioPagina = 20)
    {
        try
        {
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

            var resultado = await _chatService.GetMensajesAsync(
                chatId,
                usuarioId,
                pagina,
                tamanioPagina);

            if (!resultado.Exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Chat {chatId} no encontrado"
                });
            }

            if (!resultado.Authorized)
            {
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = "No tienes permiso para consultar los mensajes de este chat"
                });
            }

            return Ok(new ApiResponse<ChatMensajesPaginadosDto>
            {
                Success = true,
                Data = resultado.Mensajes,
                Message = "Mensajes obtenidos correctamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mensajes del chat {ChatId}", chatId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al obtener los mensajes del chat"
            });
        }
    }
}
