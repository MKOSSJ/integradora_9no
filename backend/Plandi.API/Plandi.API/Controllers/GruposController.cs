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
    public class GruposController : ControllerBase
    {
        private readonly IGrupoService _grupoService;

        public GruposController(IGrupoService grupoService)
        {
            _grupoService = grupoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _grupoService.GetAll();
                return Ok(ApiResponse<IEnumerable<GrupoResponseDto>>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<IEnumerable<GrupoResponseDto>>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<IEnumerable<GrupoResponseDto>>.Fail("Ocurrió un error interno al obtener los grupos."));
            }
        }

        [HttpGet("{publicId:guid}")]
        public async Task<IActionResult> GetById(Guid publicId)
        {
            try
            {
                var result = await _grupoService.GetById(publicId);
                return Ok(ApiResponse<GrupoResponseDto>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<GrupoResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<GrupoResponseDto>.Fail("Ocurrió un error interno al obtener el grupo."));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Create([FromBody] GrupoRequestDto request)
        {
            try
            {
                var result = await _grupoService.Create(request);
                return Ok(ApiResponse<GrupoResponseDto>.Ok(result, "Grupo creado correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<GrupoResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<GrupoResponseDto>.Fail("Ocurrió un error interno al crear el grupo."));
            }
        }

        [HttpPut("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Update(Guid publicId, [FromBody] GrupoRequestDto request)
        {
            try
            {
                var result = await _grupoService.Update(publicId, request);
                return Ok(ApiResponse<GrupoResponseDto>.Ok(result, "Grupo actualizado correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<GrupoResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<GrupoResponseDto>.Fail("Ocurrió un error interno al actualizar el grupo."));
            }
        }

        [HttpDelete("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Delete(Guid publicId)
        {
            try
            {
                var result = await _grupoService.Delete(publicId);
                return Ok(ApiResponse<bool>.Ok(result, "Grupo eliminado correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Ocurrió un error interno al eliminar el grupo."));
            }
        }
    }
}
