using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.API.Models;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/plantillas/planeacion")]
public sealed class PlantillasPlaneacionController(IAutorizacionService autorizacion, IPlaneacionTemplateService templates) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<PlantillaPlaneacionDto>>> Subir([FromForm] SubirPlantillaPlaneacionForm request, CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file is null || file.Length == 0) return BadRequest(ApiResponse<PlantillaPlaneacionDto>.Fail("Debe adjuntar una plantilla DOCX."));
        await using var stream = file.OpenReadStream();
        var result = await templates.SubirAsync(stream, file.FileName, file.ContentType, UsuarioId, cancellationToken);
        return CreatedAtAction(nameof(Activa), ApiResponse<PlantillaPlaneacionDto>.Ok(result, "Plantilla cargada y activada."));
    }

    [HttpGet("activa")]
    public async Task<ActionResult<ApiResponse<PlantillaPlaneacionDto>>> Activa(CancellationToken cancellationToken)
    {
        await ExigirDirectorAsync(cancellationToken);
        var template = await templates.ObtenerActivaAsync(cancellationToken);
        return template is null ? NotFound(ApiResponse<PlantillaPlaneacionDto>.Fail("No existe una plantilla activa.")) : Ok(ApiResponse<PlantillaPlaneacionDto>.Ok(template));
    }

    [HttpGet("{publicId:guid}/archivo")]
    public async Task<IActionResult> Visualizar(Guid publicId, CancellationToken cancellationToken) => await ArchivoAsync(publicId, false, cancellationToken);
    [HttpGet("{publicId:guid}/archivo/descarga")]
    public async Task<IActionResult> Descargar(Guid publicId, CancellationToken cancellationToken) => await ArchivoAsync(publicId, true, cancellationToken);

    private async Task<IActionResult> ArchivoAsync(Guid id, bool descargar, CancellationToken ct)
    {
        await ExigirDirectorAsync(ct);
        var file = await templates.ObtenerArchivoAsync(id, ct);
        return descargar ? File(file.Bytes, file.MimeType, file.NombreDescarga) : File(file.Bytes, file.MimeType);
    }
    private Task ExigirDirectorAsync(CancellationToken ct) => autorizacion.ExigirRolAsync(UsuarioId, RolAutorizacion.Director, ct);
    private long UsuarioId => autorizacion.ObtenerUsuarioId(User);
}
