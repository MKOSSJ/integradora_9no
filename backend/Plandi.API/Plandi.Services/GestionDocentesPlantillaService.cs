using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class GestionDocentesPlantillaService(AppDbContext context, IAutorizacionService autorizacion) : IGestionDocentesPlantillaService
{
    public async Task<CredencialesDocenteDto> CompletarCredencialesAsync(
        Guid usuarioPublicId,
        CompletarCredencialesDocenteDto solicitud,
        long usuarioAutorizadoId,
        CancellationToken cancellationToken = default)
    {
        await autorizacion.ExigirRolAsync(usuarioAutorizadoId, RolAutorizacion.Director, cancellationToken);

        var usuario = await context.Usuarios
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .SingleOrDefaultAsync(u => u.PublicId == usuarioPublicId && u.Activo && u.DeletedAt == null, cancellationToken)
            ?? throw new AppException("El usuario indicado no existe o está inactivo.");

        if (!usuario.UsuarioRoles.Any(ur => ur.Rol.Nombre == "Docente" && ur.Rol.Activo && ur.Rol.DeletedAt == null))
            throw new AppException("El usuario indicado no tiene asignado el rol Docente.");

        if (usuario.Email is not null || usuario.PasswordHash is not null)
            throw new AppException("El usuario ya tiene credenciales configuradas.");

        var email = solicitud.Email.Trim().ToLowerInvariant();
        if (await context.Usuarios.AnyAsync(u => u.Id != usuario.Id && u.Email != null && u.Email.ToLower() == email, cancellationToken))
            throw new AppException("El correo electrónico ya está registrado.");

        usuario.Email = email;
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(solicitud.Password);
        usuario.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return ADto(usuario);
    }

    private static CredencialesDocenteDto ADto(Usuario usuario) => new()
    {
        UsuarioPublicId = usuario.PublicId,
        Usuario = string.Join(" ", new[] { usuario.Nombre, usuario.ApellidoPaterno, usuario.ApellidoMaterno }
            .Where(valor => !string.IsNullOrWhiteSpace(valor))),
        Email = usuario.Email!,
        CredencialesCompletas = usuario.Email is not null && usuario.PasswordHash is not null
    };
}
