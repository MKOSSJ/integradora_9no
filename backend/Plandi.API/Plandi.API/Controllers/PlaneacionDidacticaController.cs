using Microsoft.AspNetCore.Mvc;
using Plandi.Dto;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaneacionDidacticaController : ControllerBase
{
    private readonly ILogger<PlaneacionDidacticaController> _logger;
    private readonly IPlaneacionDidacticaService _planeacionDidacticaService;

    public PlaneacionDidacticaController(
        IPlaneacionDidacticaService planeacionDidacticaService,
        ILogger<PlaneacionDidacticaController> logger)
    {
        _planeacionDidacticaService = planeacionDidacticaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las planeaciones para un revisor específico.
    /// </summary>
    [HttpGet("revisor/{id}")]
    public async Task<IActionResult> GetByRevisor(int id)
    {
        try
        {
            var planeaciones = await _planeacionDidacticaService
                .GetAllPlaneacionesForIdRevisor(id);

            // ✅ Respuesta consistente: siempre devuelve { success, data, message }
            return Ok(new ApiResponse<List<PlaneacionDidacticaRevisorDto>>
            {
                Success = true,
                Data = planeaciones,
                Message = planeaciones.Count == 0
                    ? "No se encontraron planeaciones para este revisor"
                    : $"Se encontraron {planeaciones.Count} planeaciones"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener planeaciones para revisor {RevisorId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al obtener las planeaciones"
            });
        }
    }

    /// <summary>
    /// Obtiene planeaciones agrupadas por carrera para un directivo.
    /// El directivo se identifica por su ID de usuario.
    /// Flujo: directivo → AcademiaUsuario → academias → CarreraAcademia → carreras
    ///                                               ↘ PlaneacionDidactica → planeaciones
    /// </summary>
    [HttpGet("directivo/{directivoId}")]
    public async Task<IActionResult> GetByDirectivo(int directivoId)
    {
        try
        {
            var resultado = await _planeacionDidacticaService
                .GetPlaneacionesByDirectivoAsync(directivoId);

            return Ok(new ApiResponse<List<CarreraPlaneacionDto>>
            {
                Success = true,
                Data = resultado,
                Message = resultado.Count == 0
                    ? "No se encontraron planeaciones para las carreras del directivo"
                    : $"Se encontraron planeaciones para {resultado.Count} carreras"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener planeaciones del directivo {DirectivoId}", directivoId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al obtener las planeaciones del directivo"
            });
        }
    }

    /// <summary>
    /// Busca planeaciones con filtros opcionales.
    /// Ej: GET /api/PlaneacionDidactica?carreraId=1&periodoId=1&docenteId=3&estado=1
    /// Filtros: carreraId, periodoId, asignaturaId, docenteId, fechaDesde, fechaHasta, estado.
    /// Todos son opcionales — se aplican solo los presentes.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PlaneacionFilterDto filtro)
    {
        try
        {
            var planeaciones = await _planeacionDidacticaService
                .GetAllAsync(filtro);

            return Ok(new ApiResponse<List<PlaneacionDirectivoDto>>
            {
                Success = true,
                Data = planeaciones,
                Message = planeaciones.Count == 0
                    ? "No se encontraron planeaciones con los filtros especificados"
                    : $"Se encontraron {planeaciones.Count} planeaciones"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar planeaciones con filtros");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al buscar planeaciones"
            });
        }
    }

    /// <summary>
    /// Obtiene el detalle de una planeación para revisión.
    /// </summary>
    [HttpGet("{id}/detalle-revision")]
    public async Task<IActionResult> GetDetalleRevision(long id, [FromQuery] long usuarioId)
    {
        try
        {
            var resultado = await _planeacionDidacticaService
                .GetDetalleRevisionAsync(id, usuarioId);

            if (!resultado.Exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Planeación {id} no encontrada"
                });
            }

            if (!resultado.Authorized)
            {
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = "No tienes permiso para consultar el detalle de esta planeación"
                });
            }

            return Ok(new ApiResponse<PlaneacionDetalleRevisionDto>
            {
                Success = true,
                Data = resultado.Detalle,
                Message = "Detalle de planeación obtenido correctamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de revisión de planeación {PlaneacionId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al obtener el detalle de la planeación"
            });
        }
    }

    /// <summary>
    /// Registra una observación de revisor sobre una planeación o una unidad específica.
    /// </summary>
    [HttpPost("{id}/observaciones")]
    public async Task<IActionResult> CrearObservacion(
        long id,
        [FromQuery] long usuarioId,
        [FromBody] CrearPlaneacionObservacionRequestDto? request)
    {
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Comentario))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El comentario es requerido"
                });
            }

            var resultado = await _planeacionDidacticaService
                .CrearObservacionAsync(id, usuarioId, request);

            if (!resultado.Exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Planeación {id} no encontrada"
                });
            }

            if (!resultado.Authorized)
            {
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = "No tienes permiso para registrar observaciones en esta planeación"
                });
            }

            if (!resultado.UnidadValid)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unidad no encontrada para esta planeación"
                });
            }

            return Ok(new ApiResponse<PlaneacionObservacionDto>
            {
                Success = true,
                Data = resultado.Observacion,
                Message = "Observación registrada correctamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar observación de planeación {PlaneacionId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al registrar la observación"
            });
        }
    }

    /// <summary>
    /// Autoriza una planeación después de la evaluación manual/externa de criterios.
    /// </summary>
    [HttpPost("{id}/autorizar")]
    public async Task<IActionResult> Autorizar(long id, [FromQuery] long usuarioId)
    {
        try
        {
            var resultado = await _planeacionDidacticaService
                .AutorizarAsync(id, usuarioId);

            if (!resultado.Exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Planeación {id} no encontrada"
                });
            }

            if (!resultado.Authorized)
            {
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = "No tienes permiso para autorizar esta planeación"
                });
            }

            return Ok(new ApiResponse<PlaneacionEstadoDto>
            {
                Success = true,
                Data = resultado.Planeacion,
                Message = "Planeación autorizada correctamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al autorizar planeación {PlaneacionId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al autorizar la planeación"
            });
        }
    }

    /// <summary>
    /// Solicita la revisión de una planeación y crea/reutiliza su chat de revisión.
    /// </summary>
    [HttpPost("{id}/solicitar-revision")]
    public async Task<IActionResult> SolicitarRevision(long id, [FromQuery] long usuarioId)
    {
        try
        {
            var resultado = await _planeacionDidacticaService
                .SolicitarRevisionAsync(id, usuarioId);

            if (!resultado.Exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Planeación {id} no encontrada"
                });
            }

            if (!resultado.HasDocentes)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "La planeación no tiene docentes asignados"
                });
            }

            if (!resultado.Authorized)
            {
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = "No tienes permiso para solicitar revisión de esta planeación"
                });
            }

            if (!resultado.HasRevisor)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "La planeación no tiene revisor asignado"
                });
            }

            return Ok(new ApiResponse<PlaneacionRevisionSolicitadaDto>
            {
                Success = true,
                Data = resultado.Planeacion,
                Message = "Revisión solicitada correctamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al solicitar revisión de planeación {PlaneacionId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al solicitar la revisión de la planeación"
            });
        }
    }

    /// <summary>
    /// Rechaza una planeación y la regresa a EnProceso con un motivo obligatorio.
    /// </summary>
    [HttpPost("{id}/rechazar")]
    public async Task<IActionResult> Rechazar(
        long id,
        [FromQuery] long usuarioId,
        [FromBody] PlaneacionRechazarRequestDto? request)
    {
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Motivo))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El motivo es requerido"
                });
            }

            var resultado = await _planeacionDidacticaService
                .RechazarAsync(id, usuarioId, request);

            if (!resultado.Exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Planeación {id} no encontrada"
                });
            }

            if (!resultado.Authorized)
            {
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = "No tienes permiso para rechazar esta planeación"
                });
            }

            return Ok(new ApiResponse<PlaneacionRechazoDto>
            {
                Success = true,
                Data = resultado.Planeacion,
                Message = "Planeación rechazada correctamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al rechazar planeación {PlaneacionId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al rechazar la planeación"
            });
        }
    }

    /// <summary>
    /// Obtiene una planeación por ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var planeacion = await _planeacionDidacticaService
                .GetByIdForRevisorAsync(id);

            if (planeacion is null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Planeación {id} no encontrada"
                });
            }

            return Ok(new ApiResponse<PlaneacionDidacticaRevisorDto>
            {
                Success = true,
                Data = planeacion
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener planeación {PlaneacionId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno al obtener la planeación"
            });
        }
    }

}
