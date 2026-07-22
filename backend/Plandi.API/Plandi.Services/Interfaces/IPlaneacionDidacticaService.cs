using Plandi.Dto;

namespace Plandi.Services.Interfaces;

public interface IPlaneacionDidacticaService
{
    /// <summary>
    /// Obtiene una planeación didáctica por ID para el revisor.
    /// </summary>
    Task<PlaneacionDidacticaRevisorDto?> GetByIdForRevisorAsync(long id);

    /// <summary>
    /// Obtiene todas las planeaciones para el revisor.
    /// </summary>
    Task<List<PlaneacionDidacticaRevisorDto>> GetAllForRevisorAsync();

    /// <summary>
    /// Busca planeaciones con filtros opcionales: carrera, periodo, asignatura,
    /// docente, fecha (última modificación) y/o estado.
    /// Los filtros se aplican en SQL — no en memoria.
    /// </summary>
    Task<List<PlaneacionDirectivoDto>> GetAllAsync(PlaneacionFilterDto filtro);

    /// <summary>
    /// Obtiene las planeaciones agrupadas por carrera para un directivo.
    /// Las academias se filtran por AcademiaUsuario del directivo,
    /// las carreras se vinculan vía CarreraAcademia (pivot),
    /// y las planeaciones se obtienen por PlaneacionDidactica.AcademiaId.
    /// </summary>
    Task<List<CarreraPlaneacionDto>> GetPlaneacionesByDirectivoAsync(int directivoId);
    
    /// <summary>
    /// Obtiene todas las planeaciones para el revisor por su id .
    /// </summary>
    Task<List<PlaneacionDidacticaRevisorDto>> GetAllPlaneacionesForIdRevisor(int idRevisor);

    
    /// <summary>
    /// Actualiza los campos de una planeación desde un DTO.
    /// </summary>
    Task UpdateFromDtoAsync(long id, PlaneacionDidacticaRevisorDto dto);
}
