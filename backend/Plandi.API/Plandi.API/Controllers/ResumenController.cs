using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Common;
using Plandi.Dto.Resumen;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/resumen")]
public sealed class ResumenController(IResumenService resumen, IAutorizacionService autorizacion) : ControllerBase
{
    [HttpGet("resumen-dashboard")]
    [Authorize(Roles = "Director")]
    public async Task<ActionResult<ApiResponse<ResumenDashboardDto>>> Dashboard(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenDashboardDto>.Ok(await resumen.ObtenerDashboardAsync(cancellationToken)));

    [HttpGet("resumen-usuarios")]
    [Authorize(Roles = "Director")]
    public async Task<ActionResult<ApiResponse<ResumenUsuariosDto>>> Usuarios(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenUsuariosDto>.Ok(await resumen.ObtenerUsuariosAsync(cancellationToken)));

    [HttpGet("resumen-carreras")]
    [Authorize(Roles = "Director")]
    public async Task<ActionResult<ApiResponse<ResumenCarrerasDto>>> Carreras(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenCarrerasDto>.Ok(await resumen.ObtenerCarrerasAsync(cancellationToken)));

    [HttpGet("resumen-asignaturas")]
    [Authorize(Roles = "Director")]
    public async Task<ActionResult<ApiResponse<ResumenAsignaturasDto>>> Asignaturas(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenAsignaturasDto>.Ok(await resumen.ObtenerAsignaturasAsync(cancellationToken)));

    [HttpGet("resumen-ciclos-escolares")]
    [Authorize(Roles = "Director")]
    public async Task<ActionResult<ApiResponse<ResumenCiclosEscolaresDto>>> CiclosEscolares(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenCiclosEscolaresDto>.Ok(await resumen.ObtenerCiclosEscolaresAsync(cancellationToken)));

    [HttpGet("resumen-periodos")]
    [Authorize(Roles = "Director")]
    public async Task<ActionResult<ApiResponse<ResumenPeriodosDto>>> Periodos(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenPeriodosDto>.Ok(await resumen.ObtenerPeriodosAsync(cancellationToken)));

    [HttpGet("resumen-grupos")]
    [Authorize(Roles = "Director")]
    public async Task<ActionResult<ApiResponse<ResumenGruposDto>>> Grupos(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenGruposDto>.Ok(await resumen.ObtenerGruposAsync(cancellationToken)));

    [HttpGet("resumen-asignacion-academica")]
    [Authorize(Roles = "Director")]
    public async Task<ActionResult<ApiResponse<ResumenAsignacionAcademicaDto>>> AsignacionAcademica(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenAsignacionAcademicaDto>.Ok(await resumen.ObtenerAsignacionAcademicaAsync(cancellationToken)));

    [HttpGet("resumen-seguimiento-planeaciones")]
    [Authorize(Roles = "Director")]
    public async Task<ActionResult<ApiResponse<ResumenSeguimientoPlaneacionesDto>>> SeguimientoPlaneaciones(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenSeguimientoPlaneacionesDto>.Ok(await resumen.ObtenerSeguimientoPlaneacionesAsync(cancellationToken)));

    [HttpGet("resumen-dashboard-docente")]
    [Authorize(Roles = "Docente")]
    public async Task<ActionResult<ApiResponse<ResumenDashboardDocenteDto>>> DashboardDocente(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenDashboardDocenteDto>.Ok(await resumen.ObtenerDashboardDocenteAsync(UsuarioId, cancellationToken)));

    [HttpGet("resumen-planeaciones")]
    [Authorize(Roles = "Docente")]
    public async Task<ActionResult<ApiResponse<ResumenPlaneacionesDocenteDto>>> PlaneacionesDocente(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenPlaneacionesDocenteDto>.Ok(await resumen.ObtenerPlaneacionesDocenteAsync(UsuarioId, cancellationToken)));

    [HttpGet("resumen-dashboard-revisor")]
    [Authorize(Roles = "Revisor")]
    public async Task<ActionResult<ApiResponse<ResumenDashboardRevisorDto>>> DashboardRevisor(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenDashboardRevisorDto>.Ok(await resumen.ObtenerDashboardRevisorAsync(UsuarioId, cancellationToken)));

    [HttpGet("resumen-validacion")]
    [Authorize(Roles = "Revisor")]
    public async Task<ActionResult<ApiResponse<ResumenValidacionDto>>> Validacion(CancellationToken cancellationToken) => Ok(ApiResponse<ResumenValidacionDto>.Ok(await resumen.ObtenerValidacionAsync(UsuarioId, cancellationToken)));

    private long UsuarioId => autorizacion.ObtenerUsuarioId(User);
}
