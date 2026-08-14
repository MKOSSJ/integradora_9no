using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/docentes-plantilla")]
public sealed class GestionDocentesPlantillaController(
    IGestionDocentesPlantillaService gestionDocentes,
    IAutorizacionService autorizacion) : ControllerBase
{
    [HttpPut("{usuarioPublicId:guid}/credenciales")]
    public async Task<ActionResult<ApiResponse<CredencialesDocenteDto>>> CompletarCredenciales(
        Guid usuarioPublicId,
        [FromBody] CompletarCredencialesDocenteDto solicitud,
        CancellationToken cancellationToken) =>
        Ok(ApiResponse<CredencialesDocenteDto>.Ok(
            await gestionDocentes.CompletarCredencialesAsync(usuarioPublicId, solicitud, UsuarioId, cancellationToken),
            "Credenciales configuradas."));

    private long UsuarioId => autorizacion.ObtenerUsuarioId(User);
}
