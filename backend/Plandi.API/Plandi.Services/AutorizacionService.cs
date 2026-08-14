using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class AutorizacionService(AppDbContext context) : IAutorizacionService
{
    public long ObtenerUsuarioId(ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? usuario.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return long.TryParse(valor, out var usuarioId)
            ? usuarioId
            : throw new UnauthorizedAccessException("El token no contiene un identificador de usuario válido.");
    }

    public bool HasRole(ClaimsPrincipal usuario, RolAutorizacion rol) => usuario.IsInRole(Nombre(rol));

    public Task<bool> HasRoleAsync(long usuarioId, RolAutorizacion rol, CancellationToken cancellationToken = default) =>
        context.UsuarioRoles.AnyAsync(ur => ur.UsuarioId == usuarioId && ur.Usuario.Activo && ur.Usuario.DeletedAt == null &&
            ur.Rol.Nombre == Nombre(rol), cancellationToken);

    public async Task ExigirRolAsync(long usuarioId, RolAutorizacion rol, CancellationToken cancellationToken = default)
    {
        if (!await HasRoleAsync(usuarioId, rol, cancellationToken))
            throw new UnauthorizedAccessException($"El usuario no cuenta con el rol {Nombre(rol)}.");
    }

    public async Task ExigirAlgunRolAsync(long usuarioId, IEnumerable<RolAutorizacion> roles, CancellationToken cancellationToken = default)
    {
        var requeridos = roles.Distinct().Select(Nombre).ToArray();
        if (requeridos.Length == 0 || !await context.UsuarioRoles.AnyAsync(ur => ur.UsuarioId == usuarioId &&
            ur.Usuario.Activo && ur.Usuario.DeletedAt == null && requeridos.Contains(ur.Rol.Nombre), cancellationToken))
            throw new UnauthorizedAccessException("El usuario no cuenta con uno de los roles requeridos.");
    }

    private static string Nombre(RolAutorizacion rol) => rol switch
    {
        RolAutorizacion.Docente => "Docente",
        RolAutorizacion.Revisor => "Revisor",
        RolAutorizacion.Director => "Director",
        _ => throw new ArgumentOutOfRangeException(nameof(rol))
    };
}
