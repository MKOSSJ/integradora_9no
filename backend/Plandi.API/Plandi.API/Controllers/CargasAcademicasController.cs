using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CargasAcademicasController : ControllerBase
    {
        private readonly ICargaAcademicaService _cargaAcademicaService;

        public CargasAcademicasController(ICargaAcademicaService cargaAcademicaService)
        {
            _cargaAcademicaService = cargaAcademicaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _cargaAcademicaService.GetAll();
                return Ok(ApiResponse<IEnumerable<CargaAcademicaResponseDto>>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<IEnumerable<CargaAcademicaResponseDto>>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CargaAcademicaResponseDto>>.Fail("Ocurrió un error interno al obtener las cargas académicas."));
            }
        }

        [HttpGet("{publicId:guid}")]
        public async Task<IActionResult> GetById(Guid publicId)
        {
            try
            {
                var result = await _cargaAcademicaService.GetById(publicId);
                return Ok(ApiResponse<CargaAcademicaResponseDto>.Ok(result));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<CargaAcademicaResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CargaAcademicaResponseDto>.Fail("Ocurrió un error interno al obtener la carga académica."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CargaAcademicaRequestDto request)
        {
            try
            {
                var result = await _cargaAcademicaService.Create(request);
                return Ok(ApiResponse<CargaAcademicaResponseDto>.Ok(result, "Carga académica creada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<CargaAcademicaResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CargaAcademicaResponseDto>.Fail("Ocurrió un error interno al crear la carga académica."));
            }
        }

        [HttpPut("{publicId:guid}")]
        public async Task<IActionResult> Update(Guid publicId, [FromBody] CargaAcademicaRequestDto request)
        {
            try
            {
                var result = await _cargaAcademicaService.Update(publicId, request);
                return Ok(ApiResponse<CargaAcademicaResponseDto>.Ok(result, "Carga académica actualizada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<CargaAcademicaResponseDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CargaAcademicaResponseDto>.Fail("Ocurrió un error interno al actualizar la carga académica."));
            }
        }

        [HttpDelete("{publicId:guid}")]
        public async Task<IActionResult> Delete(Guid publicId)
        {
            try
            {
                var result = await _cargaAcademicaService.Delete(publicId);
                return Ok(ApiResponse<bool>.Ok(result, "Carga académica eliminada correctamente."));
            }
            catch (AppException ex)
            {
                return Conflict(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Ocurrió un error interno al eliminar la carga académica."));
            }
        }
    }
}
