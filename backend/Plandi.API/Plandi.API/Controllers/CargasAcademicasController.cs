using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;
using Plandi.API.Models;
using Plandi.Services;

namespace Plandi.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Director")]
    [Route("api/[controller]")]
    public class CargasAcademicasController : ControllerBase
    {
        private readonly ICargaAcademicaService _cargaAcademicaService;
        private readonly IImportacionCargaAcademicaService _importacionCargaAcademicaService;
        private readonly IAutorizacionService _autorizacionService;

        public CargasAcademicasController(ICargaAcademicaService cargaAcademicaService, IImportacionCargaAcademicaService importacionCargaAcademicaService, IAutorizacionService autorizacionService)
        {
            _cargaAcademicaService = cargaAcademicaService;
            _importacionCargaAcademicaService = importacionCargaAcademicaService;
            _autorizacionService = autorizacionService;
        }

        /// <summary>Importa asignaciones desde un CSV o XLSX con Asignatura, Cuatrimestre, P.E. y Docente.</summary>
        [HttpPost("importar")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ImportacionCargaAcademicaResultadoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Importar([FromForm] ImportarCargaAcademicaForm request, CancellationToken cancellationToken)
        {
            if (request.File is null || request.File.Length == 0)
                return BadRequest(ApiResponse<ImportacionCargaAcademicaResultadoDto>.Fail("Debe adjuntar un archivo CSV o XLSX no vacío."));
            if (request.File.Length > ImportacionCargaAcademicaService.MaxFileBytes)
                return BadRequest(ApiResponse<ImportacionCargaAcademicaResultadoDto>.Fail("El archivo no puede exceder 10 MB."));

            try
            {
                await using var stream = request.File.OpenReadStream();
                var result = await _importacionCargaAcademicaService.Importar(stream, request.File.FileName, request.PeriodoPublicId, UsuarioId, cancellationToken);
                return Ok(ApiResponse<ImportacionCargaAcademicaResultadoDto>.Ok(result, "Importación finalizada."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<ImportacionCargaAcademicaResultadoDto>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<ImportacionCargaAcademicaResultadoDto>.Fail("Ocurrió un error interno al importar la carga académica."));
            }
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
                var result = await _cargaAcademicaService.Create(request, UsuarioId);
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
                var result = await _cargaAcademicaService.Update(publicId, request, UsuarioId);
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
                var result = await _cargaAcademicaService.Delete(publicId, UsuarioId);
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

        private long UsuarioId => _autorizacionService.ObtenerUsuarioId(User);
    }
}
