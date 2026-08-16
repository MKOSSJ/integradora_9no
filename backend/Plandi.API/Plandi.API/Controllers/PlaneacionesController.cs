using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plandi.API.Controllers
{
    [ApiController]
    // La edición de docentes se concentra en /api/planeaciones-flujo, que valida su carga académica.
    // Estos endpoints heredados quedan reservados para administración para evitar saltar esa validación.
    [Authorize(Roles = "Director")]
    [Route("api/[controller]")]
    public class PlaneacionesController : ControllerBase
    {
        private readonly IPlaneacionCaratulaService _caratulaService;
        private readonly IPlaneacionTemaService _temaService;
        private readonly IPlaneacionEvaluacionService _evaluacionService;
        private readonly IPlaneacionSecuenciaService _secuenciaService;
        private readonly IPlaneacionReferenciaService _referenciaService;
        private readonly IGeneracionPlaneacionesService _generacionPlaneacionesService;

        public PlaneacionesController(
            IPlaneacionCaratulaService caratulaService,
            IPlaneacionTemaService temaService,
            IPlaneacionEvaluacionService evaluacionService,
            IPlaneacionSecuenciaService secuenciaService,
            IPlaneacionReferenciaService referenciaService,
            IGeneracionPlaneacionesService generacionPlaneacionesService)
        {
            _caratulaService = caratulaService;
            _temaService = temaService;
            _evaluacionService = evaluacionService;
            _secuenciaService = secuenciaService;
            _referenciaService = referenciaService;
            _generacionPlaneacionesService = generacionPlaneacionesService;
        }

        [HttpPost("generar")]
        public async Task<ActionResult<ApiResponse<GeneracionPlaneacionesResultadoDto>>> Generar(CancellationToken cancellationToken)
        {
            var resultado = await _generacionPlaneacionesService.GenerarAsync(cancellationToken);
            return Ok(ApiResponse<GeneracionPlaneacionesResultadoDto>.Ok(resultado, "Generación de planeaciones finalizada."));
        }

        // ========== CARÁTULA ENDPOINTS ==========

        [HttpGet("caratula/{planeacionId}")]
        public async Task<ActionResult<ApiResponse<PlaneacionCaratulaDto>>> GetCaratula(Guid planeacionId)
        {
            try
            {
                var caratula = await _caratulaService.GetByPlaneacionIdAsync(planeacionId);
                if (caratula == null)
                    return NotFound(new ApiResponse<PlaneacionCaratulaDto> { Success = false, Message = "Carátula no encontrada" });

                return Ok(new ApiResponse<PlaneacionCaratulaDto> { Success = true, Data = caratula });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<PlaneacionCaratulaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("caratula/{planeacionId}")]
        public async Task<ActionResult<ApiResponse<PlaneacionCaratulaDto>>> CreateCaratula(Guid planeacionId, [FromBody] CreatePlaneacionCaratulaDto dto)
        {
            try
            {
                var caratula = await _caratulaService.CreateAsync(planeacionId, dto);
                return CreatedAtAction(nameof(GetCaratula), new { planeacionId },
                    new ApiResponse<PlaneacionCaratulaDto> { Success = true, Data = caratula });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PlaneacionCaratulaDto> { Success = false, Message = ex.Message });
            }
            catch (ConflictException ex)
            {
                return Conflict(new ApiResponse<PlaneacionCaratulaDto> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<PlaneacionCaratulaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("caratula/{id}")]
        public async Task<ActionResult<ApiResponse<PlaneacionCaratulaDto>>> UpdateCaratula(Guid id, [FromBody] UpdatePlaneacionCaratulaDto dto)
        {
            try
            {
                var caratula = await _caratulaService.UpdateAsync(id, dto);
                return Ok(new ApiResponse<PlaneacionCaratulaDto> { Success = true, Data = caratula });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<PlaneacionCaratulaDto> { Success = false, Message = ex.Message });
            }
            catch (ConflictException ex)
            {
                return Conflict(new ApiResponse<PlaneacionCaratulaDto> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<PlaneacionCaratulaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("caratula/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCaratula(Guid id)
        {
            try
            {
                await _caratulaService.DeleteAsync(id);
                return Ok(new ApiResponse<string> { Success = true, Message = "Carátula eliminada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
            catch (ConflictException ex)
            {
                return Conflict(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        // ========== TEMAS ENDPOINTS ==========

        [HttpGet("temas/{id}")]
        public async Task<ActionResult<ApiResponse<PlaneacionTemaDto>>> GetTema(Guid id)
        {
            try
            {
                var tema = await _temaService.GetByIdAsync(id);
                return Ok(new ApiResponse<PlaneacionTemaDto> { Success = true, Data = tema });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<PlaneacionTemaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("unidad/{unidadId}/temas")]
        public async Task<ActionResult<ApiResponse<List<PlaneacionTemaDto>>>> GetTemasByUnidad(Guid unidadId)
        {
            try
            {
                var temas = await _temaService.GetByUnidadIdAsync(unidadId);
                return Ok(new ApiResponse<List<PlaneacionTemaDto>> { Success = true, Data = temas });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<PlaneacionTemaDto>> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("unidad/{unidadId}/temas")]
        public async Task<ActionResult<ApiResponse<PlaneacionTemaDto>>> CreateTema(Guid unidadId, [FromBody] CreatePlaneacionTemaDtos dto)
        {
            try
            {
                var tema = await _temaService.CreateAsync(unidadId, dto);
                return CreatedAtAction(nameof(GetTema), new { id = tema.PublicId },
                    new ApiResponse<PlaneacionTemaDto> { Success = true, Data = tema });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PlaneacionTemaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("temas/{id}")]
        public async Task<ActionResult<ApiResponse<PlaneacionTemaDto>>> UpdateTema(Guid id, [FromBody] UpdatePlaneacionTemaDtos dto)
        {
            try
            {
                var tema = await _temaService.UpdateAsync(id, dto);
                return Ok(new ApiResponse<PlaneacionTemaDto> { Success = true, Data = tema });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<PlaneacionTemaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("temas/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteTema(Guid id)
        {
            try
            {
                await _temaService.DeleteAsync(id);
                return Ok(new ApiResponse<string> { Success = true, Message = "Tema eliminado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        // ========== EVALUACIONES ENDPOINTS ==========

        [HttpGet("evaluaciones/{id}")]
        public async Task<ActionResult<ApiResponse<PlaneacionEvaluacionDto>>> GetEvaluacion(Guid id)
        {
            try
            {
                var evaluacion = await _evaluacionService.GetByIdAsync(id);
                return Ok(new ApiResponse<PlaneacionEvaluacionDto> { Success = true, Data = evaluacion });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<PlaneacionEvaluacionDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("unidad/{unidadId}/evaluaciones")]
        public async Task<ActionResult<ApiResponse<List<PlaneacionEvaluacionDto>>>> GetEvaluacionesByUnidad(Guid unidadId)
        {
            try
            {
                var evaluaciones = await _evaluacionService.GetByUnidadIdAsync(unidadId);
                return Ok(new ApiResponse<List<PlaneacionEvaluacionDto>> { Success = true, Data = evaluaciones });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<PlaneacionEvaluacionDto>> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("unidad/{unidadId}/evaluaciones")]
        public async Task<ActionResult<ApiResponse<PlaneacionEvaluacionDto>>> CreateEvaluacion(Guid unidadId, [FromBody] CreatePlaneacionEvaluacionDto dto)
        {
            try
            {
                var evaluacion = await _evaluacionService.CreateAsync(unidadId, dto);
                return CreatedAtAction(nameof(GetEvaluacion), new { id = evaluacion.PublicId },
                    new ApiResponse<PlaneacionEvaluacionDto> { Success = true, Data = evaluacion });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PlaneacionEvaluacionDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("evaluaciones/{id}")]
        public async Task<ActionResult<ApiResponse<PlaneacionEvaluacionDto>>> UpdateEvaluacion(Guid id, [FromBody] UpdatePlaneacionEvaluacionDto dto)
        {
            try
            {
                var evaluacion = await _evaluacionService.UpdateAsync(id, dto);
                return Ok(new ApiResponse<PlaneacionEvaluacionDto> { Success = true, Data = evaluacion });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<PlaneacionEvaluacionDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("evaluaciones/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteEvaluacion(Guid id)
        {
            try
            {
                await _evaluacionService.DeleteAsync(id);
                return Ok(new ApiResponse<string> { Success = true, Message = "Evaluación eliminada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        // ========== SECUENCIAS ENDPOINTS ==========

        [HttpGet("secuencias/{id}")]
        public async Task<ActionResult<ApiResponse<PlaneacionSecuenciaDto>>> GetSecuencia(Guid id)
        {
            try
            {
                var secuencia = await _secuenciaService.GetByIdAsync(id);
                return Ok(new ApiResponse<PlaneacionSecuenciaDto> { Success = true, Data = secuencia });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<PlaneacionSecuenciaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("unidad/{unidadId}/secuencias")]
        public async Task<ActionResult<ApiResponse<List<PlaneacionSecuenciaDto>>>> GetSecuenciasByUnidad(Guid unidadId)
        {
            try
            {
                var secuencias = await _secuenciaService.GetByUnidadIdAsync(unidadId);
                return Ok(new ApiResponse<List<PlaneacionSecuenciaDto>> { Success = true, Data = secuencias });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<PlaneacionSecuenciaDto>> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("unidad/{unidadId}/secuencias")]
        public async Task<ActionResult<ApiResponse<PlaneacionSecuenciaDto>>> CreateSecuencia(Guid unidadId, [FromBody] CreatePlaneacionSecuenciaDto dto)
        {
            try
            {
                var secuencia = await _secuenciaService.CreateAsync(unidadId, dto);
                return CreatedAtAction(nameof(GetSecuencia), new { id = secuencia.PublicId },
                    new ApiResponse<PlaneacionSecuenciaDto> { Success = true, Data = secuencia });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PlaneacionSecuenciaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("secuencias/{id}")]
        public async Task<ActionResult<ApiResponse<PlaneacionSecuenciaDto>>> UpdateSecuencia(Guid id, [FromBody] UpdatePlaneacionSecuenciaDto dto)
        {
            try
            {
                var secuencia = await _secuenciaService.UpdateAsync(id, dto);
                return Ok(new ApiResponse<PlaneacionSecuenciaDto> { Success = true, Data = secuencia });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<PlaneacionSecuenciaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("secuencias/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteSecuencia(Guid id)
        {
            try
            {
                await _secuenciaService.DeleteAsync(id);
                return Ok(new ApiResponse<string> { Success = true, Message = "Secuencia eliminada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        // ========== REFERENCIAS ENDPOINTS ==========

        [HttpGet("referencias/{id}")]
        public async Task<ActionResult<ApiResponse<PlaneacionReferenciaDto>>> GetReferencia(Guid id)
        {
            try
            {
                var referencia = await _referenciaService.GetByIdAsync(id);
                return Ok(new ApiResponse<PlaneacionReferenciaDto> { Success = true, Data = referencia });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<PlaneacionReferenciaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("planeacion/{planeacionId}/referencias")]
        public async Task<ActionResult<ApiResponse<List<PlaneacionReferenciaDto>>>> GetReferenciasByPlaneacion(Guid planeacionId)
        {
            try
            {
                var referencias = await _referenciaService.GetByPlaneacionIdAsync(planeacionId);
                return Ok(new ApiResponse<List<PlaneacionReferenciaDto>> { Success = true, Data = referencias });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<PlaneacionReferenciaDto>> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("planeacion/{planeacionId}/referencias")]
        public async Task<ActionResult<ApiResponse<PlaneacionReferenciaDto>>> CreateReferencia(Guid planeacionId, [FromBody] CreatePlaneacionReferenciaDto dto)
        {
            try
            {
                var referencia = await _referenciaService.CreateAsync(planeacionId, dto);
                return CreatedAtAction(nameof(GetReferencia), new { id = referencia.PublicId },
                    new ApiResponse<PlaneacionReferenciaDto> { Success = true, Data = referencia });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PlaneacionReferenciaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("referencias/{id}")]
        public async Task<ActionResult<ApiResponse<PlaneacionReferenciaDto>>> UpdateReferencia(Guid id, [FromBody] UpdatePlaneacionReferenciaDto dto)
        {
            try
            {
                var referencia = await _referenciaService.UpdateAsync(id, dto);
                return Ok(new ApiResponse<PlaneacionReferenciaDto> { Success = true, Data = referencia });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<PlaneacionReferenciaDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("referencias/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteReferencia(Guid id)
        {
            try
            {
                await _referenciaService.DeleteAsync(id);
                return Ok(new ApiResponse<string> { Success = true, Message = "Referencia eliminada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }
    }
}
