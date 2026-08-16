using Plandi.Dto;

namespace Plandi.Services.Interfaces;

public interface INotificacionService
{
    Task NotificarPlaneacionAutorizadaAsync(long planeacionDidacticaId);

    Task NotificarPlaneacionRechazadaAsync(long planeacionDidacticaId, string motivo);

    Task<NotificacionesPaginadasDto> ListarAsync(long usuarioId, int pagina = 1, int tamanioPagina = 20);

    Task<(NotificacionDto? Notificacion, bool Exists, bool Authorized)> MarcarComoLeidaAsync(long id, long usuarioId);
}
