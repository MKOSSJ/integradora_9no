using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PeriodosController : ControllerBase
    {
        private readonly IPeriodoService _periodoService;
        private readonly IAutorizacionService _autorizacionService;

        public PeriodosController(IPeriodoService periodoService, IAutorizacionService autorizacionService)
        {
            _periodoService = periodoService;
            _autorizacionService = autorizacionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _periodoService.GetAll();
                return Ok(ApiResponse<IEnumerable<PeriodoResponseDto>>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<IEnumerable<PeriodoResponseDto>>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<IEnumerable<PeriodoResponseDto>>.Fail("Ocurrió un error interno al obtener los periodos."));
            }
        }

        [HttpGet("{publicId:guid}")]
        public async Task<IActionResult> GetById(Guid publicId)
        {
            try
            {
                var result = await _periodoService.GetById(publicId);
                return Ok(ApiResponse<PeriodoResponseDto>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<PeriodoResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<PeriodoResponseDto>.Fail("Ocurrió un error interno al obtener el periodo."));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Create([FromBody] PeriodoRequestDto request)
        {
            try
            {
                var result = await _periodoService.Create(request, UsuarioId);
                return Ok(ApiResponse<PeriodoResponseDto>.Ok(result, "Periodo creado correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<PeriodoResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<PeriodoResponseDto>.Fail("Ocurrió un error interno al crear el periodo."));
            }
        }

        [HttpPut("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Update(Guid publicId, [FromBody] PeriodoRequestDto request)
        {
            try
            {
                var result = await _periodoService.Update(publicId, request, UsuarioId);
                return Ok(ApiResponse<PeriodoResponseDto>.Ok(result, "Periodo actualizado correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<PeriodoResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<PeriodoResponseDto>.Fail("Ocurrió un error interno al actualizar el periodo."));
            }
        }

        [HttpDelete("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Delete(Guid publicId)
        {
            try
            {
                var result = await _periodoService.Delete(publicId, UsuarioId);
                return Ok(ApiResponse<bool>.Ok(result, "Periodo eliminado correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Ocurrió un error interno al eliminar el periodo."));
            }
        }

        private long UsuarioId => _autorizacionService.ObtenerUsuarioId(User);
    }
}
