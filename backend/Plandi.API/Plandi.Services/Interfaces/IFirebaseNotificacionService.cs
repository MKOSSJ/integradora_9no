using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces;

public interface IFirebaseNotificacionService
{
    Task RegistrarTokenAsync(long usuarioId, RegistrarDispositivoDto dto, CancellationToken cancellationToken = default);

    Task DesactivarTokenAsync(string fcmToken, CancellationToken cancellationToken = default);

    Task<bool> SendNotificationAsync(long usuarioId, string titulo, string mensaje, IReadOnlyDictionary<string, string>? data = null, CancellationToken cancellationToken = default);

    Task<int> SendNotificationToUsersAsync(IEnumerable<long> usuariosIds, string titulo, string mensaje, IReadOnlyDictionary<string, string>? data = null, CancellationToken cancellationToken = default);

    Task<bool> SaludarUsuarioAsync(long usuarioId, CancellationToken cancellationToken = default) =>
        SendNotificationAsync(usuarioId, "¡Hola desde Plandi!", "Esta es una notificación de prueba para verificar la integración con Firebase.", new Dictionary<string, string> { ["tipo"] = "SALUDO_PRUEBA" }, cancellationToken);
}
