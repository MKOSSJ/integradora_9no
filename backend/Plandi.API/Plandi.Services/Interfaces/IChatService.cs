using Plandi.Dto;

namespace Plandi.Services.Interfaces;

public interface IChatService
{
    /// <summary>
    /// Obtiene el historial paginado de mensajes de un chat para un participante activo.
    /// </summary>
    Task<(ChatMensajesPaginadosDto? Mensajes, bool Exists, bool Authorized)> GetMensajesAsync(
        long chatId,
        long usuarioId,
        int pagina,
        int tamanioPagina);
}
