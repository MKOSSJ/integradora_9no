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
    public class CarrerasController : ControllerBase
    {
        private readonly ICarreraService _carreraService;
        private readonly IAutorizacionService _autorizacionService;

        public CarrerasController(ICarreraService carreraService, IAutorizacionService autorizacionService)
        {
            _carreraService = carreraService;
            _autorizacionService = autorizacionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _carreraService.GetAll();
                return Ok(ApiResponse<IEnumerable<CarreraResponseDto>>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<IEnumerable<CarreraResponseDto>>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CarreraResponseDto>>.Fail("Ocurrió un error interno al obtener las carreras."));
            }
        }

        [HttpGet("{publicId:guid}")]
        public async Task<IActionResult> GetById(Guid publicId)
        {
            try
            {
                var result = await _carreraService.GetById(publicId);
                return Ok(ApiResponse<CarreraResponseDto>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<CarreraResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CarreraResponseDto>.Fail("Ocurrió un error interno al obtener la carrera."));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Create([FromBody] CarreraRequestDto request)
        {
            try
            {
                var result = await _carreraService.Create(request, UsuarioId);
                return Ok(ApiResponse<CarreraResponseDto>.Ok(result, "Carrera creada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<CarreraResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CarreraResponseDto>.Fail("Ocurrió un error interno al crear la carrera."));
            }
        }

        [HttpPut("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Update(Guid publicId, [FromBody] CarreraRequestDto request)
        {
            try
            {
                var result = await _carreraService.Update(publicId, request, UsuarioId);
                return Ok(ApiResponse<CarreraResponseDto>.Ok(result, "Carrera actualizada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<CarreraResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CarreraResponseDto>.Fail("Ocurrió un error interno al actualizar la carrera."));
            }
        }

        [HttpDelete("{publicId:guid}")]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Delete(Guid publicId)
        {
            try
            {
                var result = await _carreraService.Delete(publicId, UsuarioId);
                return Ok(ApiResponse<bool>.Ok(result, "Carrera eliminada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Ocurrió un error interno al eliminar la carrera."));
            }
        }

        private long UsuarioId => _autorizacionService.ObtenerUsuarioId(User);
    }
}
