using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plandi.Dto;
using Plandi.Library.Models;
using Plandi.Services.Hubs;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public class NotificacionService : INotificacionService
{
    private const int MaxTamanioPagina = 100;
    private const string TipoPlaneacionAutorizada = "PlaneacionAutorizada";
    private const string TipoPlaneacionRechazada = "PlaneacionRechazada";

    private readonly AppDbContext _dbContext;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificacionService> _logger;

    public NotificacionService(
        AppDbContext dbContext,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificacionService> logger)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task NotificarPlaneacionAutorizadaAsync(long planeacionDidacticaId)
        => CrearNotificacionesPlaneacionAsync(
            planeacionDidacticaId,
            "Planeación autorizada",
            "Tu planeación didáctica fue autorizada.",
            TipoPlaneacionAutorizada);

    public Task NotificarPlaneacionRechazadaAsync(long planeacionDidacticaId, string motivo)
    {
        var mensaje = string.IsNullOrWhiteSpace(motivo)
            ? "Tu planeación didáctica fue rechazada."
            : $"Tu planeación didáctica fue rechazada. Motivo: {motivo.Trim()}";

        return CrearNotificacionesPlaneacionAsync(
            planeacionDidacticaId,
            "Planeación rechazada",
            mensaje,
            TipoPlaneacionRechazada);
    }

    public async Task<NotificacionesPaginadasDto> ListarAsync(long usuarioId, int pagina = 1, int tamanioPagina = 20)
    {
        var tamanioPaginaSeguro = Math.Min(tamanioPagina, MaxTamanioPagina);
        var query = _dbContext.Notificaciones
            .AsNoTracking()
            .Where(n => n.UsuarioId == usuarioId && n.Activo);

        var total = await query.CountAsync();
        var totalPaginas = total == 0
            ? 0
            : (int)Math.Ceiling(total / (double)tamanioPaginaSeguro);

        var notificaciones = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Skip((pagina - 1) * tamanioPaginaSeguro)
            .Take(tamanioPaginaSeguro)
            .Select(n => new NotificacionDto
            {
                Id = n.Id,
                UsuarioId = n.UsuarioId,
                PlaneacionDidacticaId = n.PlaneacionDidacticaId,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                Tipo = n.Tipo,
                Leida = n.Leida,
                FechaLectura = n.FechaLectura,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return new NotificacionesPaginadasDto
        {
            Pagina = pagina,
            TamanioPagina = tamanioPaginaSeguro,
            Total = total,
            TotalPaginas = totalPaginas,
            HasNext = pagina < totalPaginas,
            HasPrevious = pagina > 1,
            Notificaciones = notificaciones
        };
    }

    public async Task<(NotificacionDto? Notificacion, bool Exists, bool Authorized)> MarcarComoLeidaAsync(long id, long usuarioId)
    {
        var notificacion = await _dbContext.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == id && n.Activo);

        if (notificacion is null)
        {
            return (null, false, false);
        }

        if (notificacion.UsuarioId != usuarioId)
        {
            return (null, true, false);
        }

        if (!notificacion.Leida)
        {
            var now = DateTime.UtcNow;
            notificacion.Leida = true;
            notificacion.FechaLectura = now;
            notificacion.UpdatedAt = now;

            await _dbContext.SaveChangesAsync();
        }

        return (ToDto(notificacion), true, true);
    }

    private async Task CrearNotificacionesPlaneacionAsync(
        long planeacionDidacticaId,
        string titulo,
        string mensaje,
        string tipo)
    {
        var planeacion = await _dbContext.PlaneacionesDidacticas
            .AsNoTracking()
            .Where(p => p.Id == planeacionDidacticaId)
            .Select(p => new
            {
                p.Id,
                p.Titulo
            })
            .FirstOrDefaultAsync();

        if (planeacion is null)
        {
            return;
        }

        var docenteIds = await _dbContext.PlaneacionDocentes
            .AsNoTracking()
            .Where(pd => pd.PlaneacionDidacticaId == planeacionDidacticaId && pd.Activo)
            .Select(pd => pd.DocenteId)
            .Distinct()
            .ToListAsync();

        if (docenteIds.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var mensajeConTitulo = $"{mensaje} Planeación: {planeacion.Titulo}";
        var notificaciones = docenteIds.Select(docenteId => new Notificacion
        {
            UsuarioId = docenteId,
            PlaneacionDidacticaId = planeacion.Id,
            Titulo = titulo,
            Mensaje = mensajeConTitulo,
            Tipo = tipo,
            CreatedAt = now
        }).ToList();

        _dbContext.Notificaciones.AddRange(notificaciones);
        await _dbContext.SaveChangesAsync();

        foreach (var notificacion in notificaciones)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"user-{notificacion.UsuarioId}")
                    .SendAsync("notificationReceived", ToDto(notificacion));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No se pudo enviar notificación realtime {NotificacionId} al usuario {UsuarioId}",
                    notificacion.Id,
                    notificacion.UsuarioId);
            }
        }
    }

    private static NotificacionDto ToDto(Notificacion notificacion) => new()
    {
        Id = notificacion.Id,
        UsuarioId = notificacion.UsuarioId,
        PlaneacionDidacticaId = notificacion.PlaneacionDidacticaId,
        Titulo = notificacion.Titulo,
        Mensaje = notificacion.Mensaje,
        Tipo = notificacion.Tipo,
        Leida = notificacion.Leida,
        FechaLectura = notificacion.FechaLectura,
        CreatedAt = notificacion.CreatedAt
    };
}
