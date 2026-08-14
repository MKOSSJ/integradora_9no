using System.Security.Claims;
using Plandi.Dto.Enums;

namespace Plandi.Services.Interfaces;

public interface IAutorizacionService
{
    long ObtenerUsuarioId(ClaimsPrincipal usuario);
    bool HasRole(ClaimsPrincipal usuario, RolAutorizacion rol);
    Task<bool> HasRoleAsync(long usuarioId, RolAutorizacion rol, CancellationToken cancellationToken = default);
    Task ExigirRolAsync(long usuarioId, RolAutorizacion rol, CancellationToken cancellationToken = default);
    Task ExigirAlgunRolAsync(long usuarioId, IEnumerable<RolAutorizacion> roles, CancellationToken cancellationToken = default);
}
