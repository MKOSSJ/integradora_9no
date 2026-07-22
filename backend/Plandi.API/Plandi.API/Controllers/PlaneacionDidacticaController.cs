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
