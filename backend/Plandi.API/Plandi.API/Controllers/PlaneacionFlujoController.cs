using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/planeaciones-flujo")]
public sealed class PlaneacionFlujoController(
    IMisPlaneacionesDocenteService misPlaneaciones,
    IEdicionPlaneacionService edicion,
    IAsignacionRevisorPlaneacionService asignacionRevisor,
    IPlaneacionesRevisorService revisiones,
    IEstadoPlaneacionService estados,
    IAutorizacionService autorizacion) : ControllerBase
{
    [HttpGet("mis-planeaciones")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PlaneacionResumenDto>>>> MisPlaneaciones(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<PlaneacionResumenDto>>.Ok(await misPlaneaciones.ObtenerAsync(UsuarioId, cancellationToken)));

    [HttpGet("mis-planeaciones/{publicId:guid}")]
    public async Task<ActionResult<ApiResponse<PlaneacionEdicionDto>>> DetalleDocente(Guid publicId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PlaneacionEdicionDto>.Ok(await misPlaneaciones.ObtenerDetalleAsync(publicId, UsuarioId, cancellationToken)));

    [HttpPut("mis-planeaciones/{publicId:guid}")]
    public async Task<ActionResult<ApiResponse<PlaneacionEdicionDto>>> Actualizar(Guid publicId, [FromBody] PlaneacionEdicionDto solicitud, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PlaneacionEdicionDto>.Ok(await edicion.ActualizarAsync(publicId, UsuarioId, solicitud, cancellationToken), "Planeación actualizada."));

    [HttpPost("mis-planeaciones/{publicId:guid}/enviar-revision")]
    public async Task<ActionResult<ApiResponse<PlaneacionResumenDto>>> EnviarARevision(Guid publicId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PlaneacionResumenDto>.Ok(await estados.EnviarARevisionAsync(publicId, UsuarioId, cancellationToken), "Planeación enviada a revisión."));

    [HttpPost("{publicId:guid}/asignar-revisor")]
    public async Task<ActionResult<ApiResponse<PlaneacionResumenDto>>> AsignarRevisor(Guid publicId, [FromBody] AsignarRevisorPlaneacionDto solicitud, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PlaneacionResumenDto>.Ok(await asignacionRevisor.AsignarAsync(publicId, solicitud.RevisorPublicId, UsuarioId, cancellationToken), "Revisor asignado."));

    [HttpGet("revisiones")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PlaneacionResumenDto>>>> MisRevisiones(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<PlaneacionResumenDto>>.Ok(await revisiones.ObtenerAsync(UsuarioId, cancellationToken)));

    [HttpGet("revisiones/{publicId:guid}")]
    public async Task<ActionResult<ApiResponse<PlaneacionEdicionDto>>> DetalleRevision(Guid publicId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PlaneacionEdicionDto>.Ok(await revisiones.ObtenerDetalleAsync(publicId, UsuarioId, cancellationToken)));

    [HttpPost("revisiones/{publicId:guid}/estado")]
    public async Task<ActionResult<ApiResponse<PlaneacionResumenDto>>> ResolverRevision(Guid publicId, [FromBody] CambioEstadoPlaneacionDto solicitud, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PlaneacionResumenDto>.Ok(await estados.ResolverRevisionAsync(publicId, UsuarioId, solicitud.Estado, cancellationToken), "Estado de revisión actualizado."));

    private long UsuarioId => autorizacion.ObtenerUsuarioId(User);
}
