using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class ComentariosCorreccionService(AppDbContext context, IPeriodoLifecycleService lifecycle) : IComentariosCorreccionService
{
    public ComentariosCorreccionService(AppDbContext context) : this(context, PeriodoLifecycleService.ForContext(context)) { }
    private const string TituloChat = "Comentarios de corrección";

    public async Task<ComentarioCorreccionDto> CrearAsync(
        Guid planeacionPublicId,
        CrearComentarioCorreccionDto solicitud,
        long usuarioId,
        CancellationToken cancellationToken = default)
    {
        var mensajeNormalizado = solicitud.Mensaje?.Trim();
        if (string.IsNullOrWhiteSpace(mensajeNormalizado))
            throw new AppException("El comentario no puede estar vacío.");
        if (mensajeNormalizado.Length > 4000)
            throw new AppException("El comentario no puede exceder 4000 caracteres.");

        var planeacion = await BuscarPlaneacionAsync(planeacionPublicId, cancellationToken);
        await lifecycle.ExigirEditableAsync(planeacion.PeriodoId, cancellationToken);
        var usuario = await BuscarUsuarioConRolesAsync(usuarioId, cancellationToken);
        if (TieneRol(usuario, RolAutorizacion.Director))
            throw new AppException("El Director solo puede consultar los comentarios de corrección.");
        if (planeacion.Estado is not (EstadoPlaneacion.EnRevision or EstadoPlaneacion.CorreccionSolicitada or EstadoPlaneacion.Reabierta))
            throw new AppException("Solo pueden agregarse comentarios durante revisión, corrección solicitada o reapertura.");

        var rolEnChat = await ExigirParticipanteAsync(planeacion, usuario, cancellationToken);
        var chat = await context.Chats
            .Include(c => c.Participantes)
            .FirstOrDefaultAsync(c => c.PlaneacionDidacticaId == planeacion.Id && c.Titulo == TituloChat && c.Activo && c.DeletedAt == null, cancellationToken);

        if (chat is null)
        {
            chat = new Chat { PlaneacionDidacticaId = planeacion.Id, Titulo = TituloChat };
            context.Chats.Add(chat);
        }

        var participante = chat.Participantes.FirstOrDefault(p => p.UsuarioId == usuario.Id);
        if (participante is null)
        {
            participante = new ChatParticipante { Chat = chat, UsuarioId = usuario.Id, RolEnChat = rolEnChat };
            chat.Participantes.Add(participante);
        }
        else
        {
            participante.Activo = true;
            participante.RolEnChat = rolEnChat;
        }

        var mensaje = new ChatMensaje
        {
            Chat = chat,
            UsuarioId = usuario.Id,
            Usuario = usuario,
            Mensaje = mensajeNormalizado
        };
        chat.Mensajes.Add(mensaje);
        await context.SaveChangesAsync(cancellationToken);

        return ADto(mensaje, rolEnChat);
    }

    public async Task<ComentariosCorreccionDto> ListarAsync(
        Guid planeacionPublicId,
        long usuarioId,
        CancellationToken cancellationToken = default)
    {
        var planeacion = await BuscarPlaneacionAsync(planeacionPublicId, cancellationToken);
        var usuario = await BuscarUsuarioConRolesAsync(usuarioId, cancellationToken);
        if (!TieneRol(usuario, RolAutorizacion.Director))
            await ExigirParticipanteAsync(planeacion, usuario, cancellationToken);

        if (planeacion.Estado == EstadoPlaneacion.Aprobada)
            return new ComentariosCorreccionDto
            {
                EstadoPlaneacion = planeacion.Estado,
                OcultosPorAprobacion = true
            };

        var chat = await context.Chats.AsNoTracking()
            .Include(c => c.Participantes)
            .Include(c => c.Mensajes.Where(m => m.Activo && m.DeletedAt == null && m.EliminadoAt == null))
            .ThenInclude(m => m.Usuario)
            .FirstOrDefaultAsync(c => c.PlaneacionDidacticaId == planeacion.Id && c.Titulo == TituloChat && c.Activo && c.DeletedAt == null, cancellationToken);

        return new ComentariosCorreccionDto
        {
            EstadoPlaneacion = planeacion.Estado,
            OcultosPorAprobacion = false,
            Comentarios = chat?.Mensajes
                .OrderBy(m => m.CreatedAt)
                .Select(m => ADto(m, chat.Participantes.FirstOrDefault(p => p.UsuarioId == m.UsuarioId)?.RolEnChat ?? string.Empty))
                .ToList() ?? []
        };
    }

    private async Task<PlaneacionDidactica> BuscarPlaneacionAsync(Guid publicId, CancellationToken cancellationToken) =>
        await context.PlaneacionesDidacticas.FirstOrDefaultAsync(
            p => p.PublicId == publicId && p.Activo && p.DeletedAt == null,
            cancellationToken)
        ?? throw new AppException("La planeación solicitada no existe.");

    private async Task<Usuario> BuscarUsuarioConRolesAsync(long usuarioId, CancellationToken cancellationToken) =>
        await context.Usuarios.Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo && u.DeletedAt == null, cancellationToken)
        ?? throw new AppException("El usuario indicado no existe o está inactivo.");

    private async Task<string> ExigirParticipanteAsync(PlaneacionDidactica planeacion, Usuario usuario, CancellationToken cancellationToken)
    {
        var esRevisor = TieneRol(usuario, RolAutorizacion.Revisor) && planeacion.RevisorId == usuario.Id;
        var esDocente = TieneRol(usuario, RolAutorizacion.Docente) && await context.CargasAcademicas.AnyAsync(c =>
            c.Activo && c.DeletedAt == null && c.DocenteId == usuario.Id &&
            c.PeriodoId == planeacion.PeriodoId && c.AsignaturaId == planeacion.AsignaturaId,
            cancellationToken);

        if (!esDocente && !esRevisor)
            throw new ForbiddenException("Solo los docentes y el revisor asignados pueden acceder a los comentarios de corrección.");

        // Un revisor asignado no puede acceder al borrador privado antes de que el docente lo envíe.
        if (esRevisor && !esDocente && planeacion.Estado is EstadoPlaneacion.Borrador or EstadoPlaneacion.EnProceso)
            throw new ForbiddenException("La planeación aún no ha sido enviada a revisión.");

        return esRevisor ? "Revisor" : "Docente";
    }

    private static bool TieneRol(Usuario usuario, RolAutorizacion rol)
    {
        var nombre = rol.ToString();
        return usuario.UsuarioRoles.Any(ur => ur.Rol.Activo && ur.Rol.DeletedAt == null && ur.Rol.Nombre == nombre);
    }

    private static ComentarioCorreccionDto ADto(ChatMensaje mensaje, string rolEnChat) => new()
    {
        PublicId = mensaje.PublicId,
        UsuarioPublicId = mensaje.Usuario.PublicId,
        Usuario = string.Join(" ", new[] { mensaje.Usuario.Nombre, mensaje.Usuario.ApellidoPaterno, mensaje.Usuario.ApellidoMaterno }
            .Where(valor => !string.IsNullOrWhiteSpace(valor))),
        RolEnChat = rolEnChat,
        Mensaje = mensaje.Mensaje,
        Fecha = mensaje.CreatedAt
    };
}
