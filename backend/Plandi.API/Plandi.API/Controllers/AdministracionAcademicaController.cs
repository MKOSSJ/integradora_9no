using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Authorize(Roles = "Director")]
[Route("api/admin")]
public sealed class AdministracionAcademicaController(
    IAdministracionAcademicaService consultas,
    ICargaAcademicaService cargas,
    IPeriodoService periodos,
    IAutorizacionService autorizacion) : ControllerBase
{
    [HttpGet("usuarios")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminUsuarioDto>>>> Usuarios([FromQuery] AdminConsultaDto filtro, CancellationToken ct) => Ok(ApiResponse<PagedResult<AdminUsuarioDto>>.Ok(await consultas.UsuariosAsync(filtro, ct)));
    [HttpGet("usuarios/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminUsuarioDto>>> Usuario(Guid id, CancellationToken ct) => Ok(ApiResponse<AdminUsuarioDto>.Ok(await consultas.UsuarioAsync(id, ct)));

    [HttpGet("grupos")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminGrupoDto>>>> Grupos([FromQuery] AdminConsultaDto filtro, CancellationToken ct) => Ok(ApiResponse<PagedResult<AdminGrupoDto>>.Ok(await consultas.GruposAsync(filtro, ct)));
    [HttpGet("grupos/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminGrupoDto>>> Grupo(Guid id, CancellationToken ct) => Ok(ApiResponse<AdminGrupoDto>.Ok(await consultas.GrupoAsync(id, ct)));

    [HttpGet("asignaturas")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminAsignaturaDto>>>> Asignaturas([FromQuery] AdminConsultaDto filtro, CancellationToken ct) => Ok(ApiResponse<PagedResult<AdminAsignaturaDto>>.Ok(await consultas.AsignaturasAsync(filtro, ct)));
    [HttpGet("asignaturas/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminAsignaturaDto>>> Asignatura(Guid id, CancellationToken ct) => Ok(ApiResponse<AdminAsignaturaDto>.Ok(await consultas.AsignaturaAsync(id, ct)));

    [HttpGet("ciclos")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminCicloDto>>>> Ciclos([FromQuery] AdminConsultaDto filtro, CancellationToken ct) => Ok(ApiResponse<PagedResult<AdminCicloDto>>.Ok(await consultas.CiclosAsync(filtro, ct)));
    [HttpGet("ciclos/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminCicloDto>>> Ciclo(Guid id, CancellationToken ct) => Ok(ApiResponse<AdminCicloDto>.Ok(await consultas.CicloAsync(id, ct)));

    [HttpGet("periodos")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminPeriodoDto>>>> Periodos([FromQuery] AdminConsultaDto filtro, CancellationToken ct) => Ok(ApiResponse<PagedResult<AdminPeriodoDto>>.Ok(await consultas.PeriodosAsync(filtro, ct)));
    [HttpGet("periodos/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminPeriodoDto>>> Periodo(Guid id, CancellationToken ct) => Ok(ApiResponse<AdminPeriodoDto>.Ok(await consultas.PeriodoAsync(id, ct)));
    [HttpPost("periodos/{id:guid}/cerrar")]
    public async Task<ActionResult<ApiResponse<PeriodoResponseDto>>> CerrarPeriodo(Guid id, CancellationToken ct) => Ok(ApiResponse<PeriodoResponseDto>.Ok(await periodos.Cerrar(id, UsuarioId, ct), "Periodo cerrado correctamente."));

    [HttpGet("cargas-academicas")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminCargaResumenDto>>>> Cargas([FromQuery] AdminConsultaDto filtro, CancellationToken ct) => Ok(ApiResponse<PagedResult<AdminCargaResumenDto>>.Ok(await consultas.CargasAsync(filtro, ct)));
    [HttpGet("cargas-academicas/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminCargaResumenDto>>> Carga(Guid id, CancellationToken ct) => Ok(ApiResponse<AdminCargaResumenDto>.Ok(await consultas.CargaAsync(id, ct)));
    [HttpPut("cargas-academicas/{id:guid}/grupo")]
    public async Task<ActionResult<ApiResponse<CargaAcademicaResponseDto>>> ActualizarGrupo(Guid id, [FromBody] ActualizarGrupoCargaAcademicaDto solicitud, CancellationToken ct) =>
        Ok(ApiResponse<CargaAcademicaResponseDto>.Ok(await cargas.UpdateGrupo(id, solicitud, UsuarioId, ct), "Grupo de la carga académica actualizado."));

    private long UsuarioId => autorizacion.ObtenerUsuarioId(User);
}
