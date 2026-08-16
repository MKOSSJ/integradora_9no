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
    public class CiclosEscolaresController : ControllerBase
    {
        private readonly ICicloEscolarService _cicloEscolarService;
        private readonly IAutorizacionService _autorizacionService;

        public CiclosEscolaresController(ICicloEscolarService cicloEscolarService, IAutorizacionService autorizacionService)
        {
            _cicloEscolarService = cicloEscolarService;
            _autorizacionService = autorizacionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _cicloEscolarService.GetAll();
                return Ok(ApiResponse<IEnumerable<CicloEscolarResponseDto>>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<IEnumerable<CicloEscolarResponseDto>>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CicloEscolarResponseDto>>.Fail("Ocurrió un error interno al obtener los ciclos escolares."));
            }
        }

        [HttpGet("{publicId:guid}")]
        public async Task<IActionResult> GetById(Guid publicId)
        {
            try
            {
                var result = await _cicloEscolarService.GetById(publicId);
                return Ok(ApiResponse<CicloEscolarResponseDto>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<CicloEscolarResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CicloEscolarResponseDto>.Fail("Ocurrió un error interno al obtener el ciclo escolar."));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Create([FromBody] CicloEscolarRequestDto request)
        {
            try
            {
                var result = await _cicloEscolarService.Create(request, UsuarioId);
                return Ok(ApiResponse<CicloEscolarResponseDto>.Ok(result, "Ciclo escolar creado correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<CicloEscolarResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CicloEscolarResponseDto>.Fail("Ocurrió un error interno al crear el ciclo escolar."));
            }
        }

        [HttpPut("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Update(Guid publicId, [FromBody] CicloEscolarRequestDto request)
        {
            try
            {
                var result = await _cicloEscolarService.Update(publicId, request, UsuarioId);
                return Ok(ApiResponse<CicloEscolarResponseDto>.Ok(result, "Ciclo escolar actualizado correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<CicloEscolarResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CicloEscolarResponseDto>.Fail("Ocurrió un error interno al actualizar el ciclo escolar."));
            }
        }

        [HttpDelete("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Delete(Guid publicId)
        {
            try
            {
                var result = await _cicloEscolarService.Delete(publicId, UsuarioId);
                return Ok(ApiResponse<bool>.Ok(result, "Ciclo escolar eliminado correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Ocurrió un error interno al eliminar el ciclo escolar."));
            }
        }

        private long UsuarioId => _autorizacionService.ObtenerUsuarioId(User);
    }
}
