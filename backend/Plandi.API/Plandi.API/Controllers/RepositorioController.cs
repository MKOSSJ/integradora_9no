using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/repositorio")]
public sealed class RepositorioController(IRepositorioService repositorio, IAutorizacionService autorizacion) : ControllerBase
{
    [HttpGet("planeaciones")]
    public async Task<ActionResult<ApiResponse<PagedResult<RepositorioPlaneacionDto>>>> Buscar([FromQuery] RepositorioPlaneacionesFiltroDto filtro, CancellationToken ct) =>
        Ok(ApiResponse<PagedResult<RepositorioPlaneacionDto>>.Ok(await repositorio.BuscarAsync(filtro, UsuarioId, ct)));

    [HttpGet("planeaciones/{id:guid}")]
    public async Task<ActionResult<ApiResponse<RepositorioPlaneacionDto>>> Obtener(Guid id, CancellationToken ct) =>
        Ok(ApiResponse<RepositorioPlaneacionDto>.Ok(await repositorio.ObtenerAsync(id, UsuarioId, ct)));

    [HttpGet("planeaciones/{id:guid}/archivos")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RepositorioArchivoDto>>>> Archivos(Guid id, CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<RepositorioArchivoDto>>.Ok(await repositorio.ArchivosAsync(id, UsuarioId, ct)));

    [HttpGet("planeaciones/{id:guid}/archivos/{tipo}/descargar")]
    public async Task<IActionResult> Descargar(Guid id, string tipo, CancellationToken ct)
    {
        var archivo = await repositorio.DescargarAsync(id, tipo, UsuarioId, ct);
        return File(archivo.Bytes, archivo.MimeType, archivo.NombreDescarga);
    }

    private long UsuarioId => autorizacion.ObtenerUsuarioId(User);
}
