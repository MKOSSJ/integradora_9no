using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/planeaciones")]
public sealed class PlaneacionDocumentosController(IAutorizacionService autorizacion, IPlaneacionDocumentosService documentos, IPlaneacionPdfService pdf) : ControllerBase
{
    [HttpGet("{publicId:guid}")]
    public async Task<ActionResult<ApiResponse<PlaneacionDetalleConArchivosDto>>> Detalle(Guid publicId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PlaneacionDetalleConArchivosDto>.Ok(await documentos.ObtenerDetalleAsync(publicId, UsuarioId, cancellationToken)));

    [HttpGet("{publicId:guid}/pdf")]
    public async Task<IActionResult> VisualizarPdf(Guid publicId, CancellationToken cancellationToken) => await ArchivoPdfAsync(publicId, false, cancellationToken);

    [HttpGet("{publicId:guid}/pdf/descarga")]
    public async Task<IActionResult> DescargarPdf(Guid publicId, CancellationToken cancellationToken) => await ArchivoPdfAsync(publicId, true, cancellationToken);

    private async Task<IActionResult> ArchivoPdfAsync(Guid publicId, bool descargar, CancellationToken cancellationToken)
    {
        try
        {
            var file = await pdf.GenerarPdfAsync(publicId, UsuarioId, cancellationToken);
            return descargar ? File(file.Bytes, file.MimeType, file.NombreDescarga) : File(file.Bytes, file.MimeType);
        }
        catch (PdfGenerationException ex) { return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(ex.Message)); }
    }
    private long UsuarioId => autorizacion.ObtenerUsuarioId(User);
}
