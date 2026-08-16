using Microsoft.EntityFrameworkCore;
using Plandi.Dto;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public class ChatService : IChatService
{
    private const int MaxTamanioPagina = 100;

    private readonly AppDbContext _dbContext;

    public ChatService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(ChatMensajesPaginadosDto? Mensajes, bool Exists, bool Authorized)> GetMensajesAsync(
        long chatId,
        long usuarioId,
        int pagina,
        int tamanioPagina)
    {
        var chatExists = await _dbContext.Chats
            .AsNoTracking()
            .AnyAsync(c => c.Id == chatId);

        if (!chatExists)
        {
            return (null, false, false);
        }

        var isParticipanteActivo = await _dbContext.ChatParticipantes
            .AsNoTracking()
            .AnyAsync(cp => cp.ChatId == chatId
                && cp.UsuarioId == usuarioId
                && cp.Activo);

        if (!isParticipanteActivo)
        {
            return (null, true, false);
        }

        var tamanioPaginaSeguro = Math.Min(tamanioPagina, MaxTamanioPagina);
        var query = _dbContext.ChatMensajes
            .AsNoTracking()
            .Where(m => m.ChatId == chatId && m.EliminadoAt == null);

        var total = await query.CountAsync();
        var totalPaginas = total == 0
            ? 0
            : (int)Math.Ceiling(total / (double)tamanioPaginaSeguro);

        var mensajesData = await query
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .Skip((pagina - 1) * tamanioPaginaSeguro)
            .Take(tamanioPaginaSeguro)
            .Select(m => new
            {
                m.Id,
                m.ChatId,
                m.UsuarioId,
                m.Mensaje,
                m.TipoMensaje,
                Fecha = m.CreatedAt,
                m.EditadoAt,
                AutorNombre = m.Usuario.Nombre,
                AutorApellidoPaterno = m.Usuario.ApellidoPaterno,
                AutorApellidoMaterno = m.Usuario.ApellidoMaterno
            })
            .ToListAsync();

        var mensajes = mensajesData.Select(m => new ChatMensajeDto
        {
            Id = m.Id,
            ChatId = m.ChatId,
            UsuarioId = m.UsuarioId,
            Autor = BuildNombreUsuario(m.AutorNombre, m.AutorApellidoPaterno, m.AutorApellidoMaterno),
            Mensaje = m.Mensaje,
            TipoMensaje = m.TipoMensaje,
            Fecha = m.Fecha,
            EditadoAt = m.EditadoAt
        }).ToList();

        return (new ChatMensajesPaginadosDto
        {
            Pagina = pagina,
            TamanioPagina = tamanioPaginaSeguro,
            Total = total,
            TotalPaginas = totalPaginas,
            HasNext = pagina < totalPaginas,
            HasPrevious = pagina > 1,
            Mensajes = mensajes
        }, true, true);
    }

    private static string BuildNombreUsuario(string nombre, string apellidoPaterno, string? apellidoMaterno)
        => string.Join(' ', new[] { nombre, apellidoPaterno, apellidoMaterno }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
}
