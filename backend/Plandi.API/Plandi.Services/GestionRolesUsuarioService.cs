using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;
using RolEntidad = Plandi.Library.Models.Rol;

namespace Plandi.Services;

public sealed class GestionRolesUsuarioService(AppDbContext context, IAutorizacionService autorizacion) : IGestionRolesUsuarioService
{
    public async Task<IReadOnlyList<RolUsuarioDto>> ObtenerCatalogoAsync(long usuarioAutorizadoId, CancellationToken cancellationToken = default)
    {
        await ExigirDirectorAsync(usuarioAutorizadoId, cancellationToken);
        return await context.Roles.AsNoTracking()
            .Where(rol => rol.Activo && rol.DeletedAt == null)
            .OrderBy(rol => rol.Nombre)
            .Select(rol => ADto(rol))
            .ToListAsync(cancellationToken);
    }

    public async Task<UsuarioRolesDto> ObtenerRolesAsync(Guid usuarioPublicId, long usuarioAutorizadoId, CancellationToken cancellationToken = default)
    {
        await ExigirDirectorAsync(usuarioAutorizadoId, cancellationToken);
        return ADto(await UsuarioConRoles(usuarioPublicId, cancellationToken));
    }

    public async Task<UsuarioRolesDto> AsignarAsync(Guid usuarioPublicId, Guid rolPublicId, long usuarioAutorizadoId, CancellationToken cancellationToken = default)
    {
        await ExigirDirectorAsync(usuarioAutorizadoId, cancellationToken);
        var usuario = await UsuarioConRoles(usuarioPublicId, cancellationToken);
        var rol = await RolActivo(rolPublicId, cancellationToken);
        if (usuario.UsuarioRoles.Any(enlace => enlace.RolId == rol.Id))
            throw new AppException("El usuario ya tiene asignado ese rol.");
        if (rol.Nombre == "Revisor" && !usuario.UsuarioRoles.Any(enlace => enlace.Rol.Nombre == "Docente" && enlace.Rol.Activo && enlace.Rol.DeletedAt == null))
            throw new AppException("Solo un usuario con rol Docente puede recibir el rol Revisor.");

        usuario.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.Id, RolId = rol.Id, Rol = rol });
        await context.SaveChangesAsync(cancellationToken);
        return ADto(usuario);
    }

    public async Task<UsuarioRolesDto> QuitarAsync(Guid usuarioPublicId, Guid rolPublicId, long usuarioAutorizadoId, CancellationToken cancellationToken = default)
    {
        await ExigirDirectorAsync(usuarioAutorizadoId, cancellationToken);
        var usuario = await UsuarioConRoles(usuarioPublicId, cancellationToken);
        var rol = await RolActivo(rolPublicId, cancellationToken);
        var enlace = usuario.UsuarioRoles.SingleOrDefault(enlace => enlace.RolId == rol.Id)
            ?? throw new AppException("El usuario no tiene asignado ese rol.");

        context.UsuarioRoles.Remove(enlace);
        await context.SaveChangesAsync(cancellationToken);
        usuario.UsuarioRoles.Remove(enlace);
        return ADto(usuario);
    }

    private Task ExigirDirectorAsync(long usuarioId, CancellationToken cancellationToken) =>
        autorizacion.ExigirRolAsync(usuarioId, RolAutorizacion.Director, cancellationToken);

    private async Task<Usuario> UsuarioConRoles(Guid usuarioPublicId, CancellationToken cancellationToken) =>
        await context.Usuarios.Include(usuario => usuario.UsuarioRoles).ThenInclude(enlace => enlace.Rol)
            .SingleOrDefaultAsync(usuario => usuario.PublicId == usuarioPublicId && usuario.Activo && usuario.DeletedAt == null, cancellationToken)
        ?? throw new AppException("El usuario indicado no existe o está inactivo.");

    private async Task<RolEntidad> RolActivo(Guid rolPublicId, CancellationToken cancellationToken) =>
        await context.Roles.SingleOrDefaultAsync(rol => rol.PublicId == rolPublicId && rol.Activo && rol.DeletedAt == null, cancellationToken)
        ?? throw new AppException("El rol indicado no existe o está inactivo.");

    private static UsuarioRolesDto ADto(Usuario usuario) => new()
    {
        UsuarioPublicId = usuario.PublicId,
        Usuario = string.Join(" ", new[] { usuario.Nombre, usuario.ApellidoPaterno, usuario.ApellidoMaterno }.Where(valor => !string.IsNullOrWhiteSpace(valor))),
        Roles = usuario.UsuarioRoles.Where(enlace => enlace.Rol.Activo && enlace.Rol.DeletedAt == null)
            .OrderBy(enlace => enlace.Rol.Nombre).Select(enlace => ADto(enlace.Rol)).ToList()
    };

    private static RolUsuarioDto ADto(RolEntidad rol) => new() { PublicId = rol.PublicId, Nombre = rol.Nombre, Descripcion = rol.Descripcion };
}
