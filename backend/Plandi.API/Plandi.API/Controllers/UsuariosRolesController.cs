using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/usuarios-roles")]
public sealed class UsuariosRolesController(
    IGestionRolesUsuarioService gestionRoles,
    IAutorizacionService autorizacion) : ControllerBase
{
    [HttpGet("catalogo")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RolUsuarioDto>>>> Catalogo(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<RolUsuarioDto>>.Ok(await gestionRoles.ObtenerCatalogoAsync(UsuarioId, cancellationToken)));

    [HttpGet("{usuarioPublicId:guid}")]
    public async Task<ActionResult<ApiResponse<UsuarioRolesDto>>> Obtener(Guid usuarioPublicId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<UsuarioRolesDto>.Ok(await gestionRoles.ObtenerRolesAsync(usuarioPublicId, UsuarioId, cancellationToken)));

    [HttpPost("{usuarioPublicId:guid}")]
    public async Task<ActionResult<ApiResponse<UsuarioRolesDto>>> Asignar(Guid usuarioPublicId, [FromBody] AsignarRolUsuarioDto solicitud, CancellationToken cancellationToken) =>
        Ok(ApiResponse<UsuarioRolesDto>.Ok(await gestionRoles.AsignarAsync(usuarioPublicId, solicitud.RolPublicId, UsuarioId, cancellationToken), "Rol asignado."));

    [HttpDelete("{usuarioPublicId:guid}/{rolPublicId:guid}")]
    public async Task<ActionResult<ApiResponse<UsuarioRolesDto>>> Quitar(Guid usuarioPublicId, Guid rolPublicId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<UsuarioRolesDto>.Ok(await gestionRoles.QuitarAsync(usuarioPublicId, rolPublicId, UsuarioId, cancellationToken), "Rol retirado."));

    private long UsuarioId => autorizacion.ObtenerUsuarioId(User);
}
