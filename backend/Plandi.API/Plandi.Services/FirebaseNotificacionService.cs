using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class FirebaseNotificacionService(
    AppDbContext context,
    ILogger<FirebaseNotificacionService> logger) : IFirebaseNotificacionService
{
    public async Task RegistrarTokenAsync(long usuarioId, RegistrarDispositivoDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.FcmToken))
            throw new AppException("El token FCM es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.DeviceType))
            throw new AppException("El tipo de dispositivo es obligatorio.");

        var tokenLimpio = dto.FcmToken.Trim();
        var tipoLimpio = dto.DeviceType.Trim();

        var tokenExistente = await context.UserDeviceTokens
            .FirstOrDefaultAsync(t => t.DeviceToken == tokenLimpio, cancellationToken);

        if (tokenExistente != null)
        {
            tokenExistente.UserId = usuarioId;
            tokenExistente.DeviceType = tipoLimpio;
            tokenExistente.Activo = true;
            tokenExistente.DeletedAt = null;
            tokenExistente.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var nuevoToken = new UserDeviceToken
            {
                UserId = usuarioId,
                DeviceToken = tokenLimpio,
                DeviceType = tipoLimpio,
                Activo = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.UserDeviceTokens.AddAsync(nuevoToken, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Dispositivo registrado exitosamente para el usuario {UsuarioId} con tipo {DeviceType}.", usuarioId, tipoLimpio);
    }

    public async Task DesactivarTokenAsync(string fcmToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fcmToken))
            return;

        var tokenLimpio = fcmToken.Trim();
        var token = await context.UserDeviceTokens
            .FirstOrDefaultAsync(t => t.DeviceToken == tokenLimpio, cancellationToken);

        if (token != null && token.Activo)
        {
            token.Activo = false;
            token.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Token FCM desactivado correctamente.");
        }
    }

    public async Task<bool> SendNotificationAsync(
        long usuarioId,
        string titulo,
        string mensaje,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        var tokens = await context.UserDeviceTokens
            .Where(t => t.UserId == usuarioId && t.Activo && t.DeletedAt == null)
            .Select(t => t.DeviceToken)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (tokens.Count == 0)
        {
            logger.LogInformation("No se encontraron dispositivos activos para el usuario {UsuarioId}.", usuarioId);
            return false;
        }

        return await EnviarMulticastTokensAsync(tokens, titulo, mensaje, data, cancellationToken);
    }

    public async Task<int> SendNotificationToUsersAsync(
        IEnumerable<long> usuariosIds,
        string titulo,
        string mensaje,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        var ids = usuariosIds.Distinct().ToList();
        if (ids.Count == 0)
            return 0;

        var tokens = await context.UserDeviceTokens
            .Where(t => ids.Contains(t.UserId) && t.Activo && t.DeletedAt == null)
            .Select(t => t.DeviceToken)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (tokens.Count == 0)
        {
            logger.LogInformation("No se encontraron dispositivos activos para los usuarios solicitados.");
            return 0;
        }

        var exito = await EnviarMulticastTokensAsync(tokens, titulo, mensaje, data, cancellationToken);
        return exito ? tokens.Count : 0;
    }

    public async Task<bool> SaludarUsuarioAsync(long usuarioId, CancellationToken cancellationToken = default)
    {
        var titulo = "¡Hola desde Plandi!";
        var mensaje = "Esta es una notificación de prueba para verificar la integración con Firebase.";
        var datos = new Dictionary<string, string>
        {
            ["tipo"] = "SALUDO_PRUEBA",
            ["timestamp"] = DateTime.UtcNow.ToString("o")
        };

        return await SendNotificationAsync(usuarioId, titulo, mensaje, datos, cancellationToken);
    }

    private async Task<bool> EnviarMulticastTokensAsync(
        IReadOnlyList<string> tokens,
        string titulo,
        string mensaje,
        IReadOnlyDictionary<string, string>? data,
        CancellationToken cancellationToken)
    {
        var messaging = FirebaseMessaging.DefaultInstance;
        if (messaging == null)
        {
            logger.LogError("FirebaseMessaging DefaultInstance es null. Verifique la inicialización de FirebaseApp.");
            return false;
        }

        var tokensInvalidos = new List<string>();
        var totalEnviados = 0;

        // Firebase soporta hasta 500 mensajes por SendEachAsync
        foreach (var chunk in tokens.Chunk(500))
        {
            var chunkList = chunk.ToList();
#pragma warning disable CS0618
            var messages = chunkList.Select(token => new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = titulo,
                    Body = mensaje
                },
                Data = data != null ? new Dictionary<string, string>(data) : null
            }).ToList();
#pragma warning restore CS0618

            try
            {
                var response = await messaging.SendEachAsync(messages, cancellationToken);
                totalEnviados += response.SuccessCount;

                for (var i = 0; i < response.Responses.Count; i++)
                {
                    var sendResponse = response.Responses[i];
                    if (!sendResponse.IsSuccess)
                    {
                        var errorCode = sendResponse.Exception?.MessagingErrorCode;
                        if (errorCode == MessagingErrorCode.Unregistered || errorCode == MessagingErrorCode.InvalidArgument)
                        {
                            tokensInvalidos.Add(chunkList[i]);
                        }

                        logger.LogWarning("Fallo en envío de notificación FCM al token: {ErrorCode} - {ErrorMessage}",
                            errorCode, sendResponse.Exception?.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al enviar notificaciones push con FCM.");
            }
        }

        if (tokensInvalidos.Count > 0)
        {
            await DesactivarTokensInvalidosAsync(tokensInvalidos, cancellationToken);
        }

        return totalEnviados > 0;
    }

    private async Task DesactivarTokensInvalidosAsync(IEnumerable<string> tokensInvalidos, CancellationToken cancellationToken)
    {
        try
        {
            var listaTokens = tokensInvalidos.Distinct().ToList();
            var tokensParaDesactivar = await context.UserDeviceTokens
                .Where(t => listaTokens.Contains(t.DeviceToken) && t.Activo)
                .ToListAsync(cancellationToken);

            if (tokensParaDesactivar.Count > 0)
            {
                foreach (var token in tokensParaDesactivar)
                {
                    token.Activo = false;
                    token.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Se desactivaron {Count} tokens no registrados o inválidos.", tokensParaDesactivar.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al desactivar tokens inválidos.");
        }
    }
}
