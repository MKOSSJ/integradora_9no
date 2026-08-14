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
    public class AcademiasController : ControllerBase
    {
        private readonly IAcademiaService _academiaService;

        public AcademiasController(IAcademiaService academiaService)
        {
            _academiaService = academiaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _academiaService.GetAll();
                return Ok(ApiResponse<IEnumerable<AcademiaResponseDto>>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<IEnumerable<AcademiaResponseDto>>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<IEnumerable<AcademiaResponseDto>>.Fail("Ocurrió un error interno al obtener las academias."));
            }
        }

        [HttpGet("{publicId:guid}")]
        public async Task<IActionResult> GetById(Guid publicId)
        {
            try
            {
                var result = await _academiaService.GetById(publicId);
                return Ok(ApiResponse<AcademiaResponseDto>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<AcademiaResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<AcademiaResponseDto>.Fail("Ocurrió un error interno al obtener la academia."));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Create([FromBody] AcademiaRequestDto request)
        {
            try
            {
                var result = await _academiaService.Create(request);
                return Ok(ApiResponse<AcademiaResponseDto>.Ok(result, "Academia creada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<AcademiaResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<AcademiaResponseDto>.Fail("Ocurrió un error interno al crear la academia."));
            }
        }

        [HttpPut("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Update(Guid publicId, [FromBody] AcademiaRequestDto request)
        {
            try
            {
                var result = await _academiaService.Update(publicId, request);
                return Ok(ApiResponse<AcademiaResponseDto>.Ok(result, "Academia actualizada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<AcademiaResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<AcademiaResponseDto>.Fail("Ocurrió un error interno al actualizar la academia."));
            }
        }

        [HttpDelete("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Delete(Guid publicId)
        {
            try
            {
                var result = await _academiaService.Delete(publicId);
                return Ok(ApiResponse<bool>.Ok(result, "Academia eliminada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Ocurrió un error interno al eliminar la academia."));
            }
        }

        [HttpGet("{publicId:guid}/usuarios")]
        public async Task<IActionResult> GetUsuarios(Guid publicId)
        {
            try
            {
                var result = await _academiaService.GetUsuarios(publicId);
                return Ok(ApiResponse<IEnumerable<AcademiaUsuarioResponseDto>>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<IEnumerable<AcademiaUsuarioResponseDto>>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<IEnumerable<AcademiaUsuarioResponseDto>>.Fail("Ocurrió un error interno al obtener los usuarios de la academia."));
            }
        }

        [HttpPost("{publicId:guid}/usuarios")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> AsignarUsuario(Guid publicId, [FromBody] AcademiaUsuarioRequestDto request)
        {
            try
            {
                var result = await _academiaService.AsignarUsuario(publicId, request);
                return Ok(ApiResponse<AcademiaUsuarioResponseDto>.Ok(result, "Usuario asignado a la academia correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<AcademiaUsuarioResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<AcademiaUsuarioResponseDto>.Fail("Ocurrió un error interno al asignar el usuario a la academia."));
            }
        }

        [HttpDelete("{publicId:guid}/usuarios/{usuarioPublicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> DesasignarUsuario(Guid publicId, Guid usuarioPublicId)
        {
            try
            {
                var result = await _academiaService.DesasignarUsuario(publicId, usuarioPublicId);
                return Ok(ApiResponse<bool>.Ok(result, "Usuario desasignado de la academia correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Ocurrió un error interno al desasignar el usuario de la academia."));
            }
        }
    }
}
