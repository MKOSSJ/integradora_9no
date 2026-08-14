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
    public class AsignaturasController : ControllerBase
    {
        private readonly IAsignaturaService _asignaturaService;
        private readonly IAutorizacionService _autorizacionService;

        public AsignaturasController(IAsignaturaService asignaturaService, IAutorizacionService autorizacionService)
        {
            _asignaturaService = asignaturaService;
            _autorizacionService = autorizacionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _asignaturaService.GetAll();
                return Ok(ApiResponse<IEnumerable<AsignaturaResponseDto>>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<IEnumerable<AsignaturaResponseDto>>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<IEnumerable<AsignaturaResponseDto>>.Fail("Ocurrió un error interno al obtener las asignaturas."));
            }
        }

        [HttpGet("{publicId:guid}")]
        public async Task<IActionResult> GetById(Guid publicId)
        {
            try
            {
                var result = await _asignaturaService.GetById(publicId);
                return Ok(ApiResponse<AsignaturaResponseDto>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<AsignaturaResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<AsignaturaResponseDto>.Fail("Ocurrió un error interno al obtener la asignatura."));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Create([FromBody] AsignaturaRequestDto request)
        {
            try
            {
                var result = await _asignaturaService.Create(request, UsuarioId);
                return Ok(ApiResponse<AsignaturaResponseDto>.Ok(result, "Asignatura creada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<AsignaturaResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<AsignaturaResponseDto>.Fail("Ocurrió un error interno al crear la asignatura."));
            }
        }

        [HttpPut("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Update(Guid publicId, [FromBody] AsignaturaRequestDto request)
        {
            try
            {
                var result = await _asignaturaService.Update(publicId, request, UsuarioId);
                return Ok(ApiResponse<AsignaturaResponseDto>.Ok(result, "Asignatura actualizada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<AsignaturaResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<AsignaturaResponseDto>.Fail("Ocurrió un error interno al actualizar la asignatura."));
            }
        }

        [HttpDelete("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Delete(Guid publicId)
        {
            try
            {
                var result = await _asignaturaService.Delete(publicId, UsuarioId);
                return Ok(ApiResponse<bool>.Ok(result, "Asignatura eliminada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Ocurrió un error interno al eliminar la asignatura."));
            }
        }

        private long UsuarioId => _autorizacionService.ObtenerUsuarioId(User);
    }
}
