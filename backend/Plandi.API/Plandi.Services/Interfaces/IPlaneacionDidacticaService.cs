using Plandi.Dto;

namespace Plandi.Services.Interfaces;

public interface IPlaneacionDidacticaService
{
    /// <summary>
    /// Obtiene una planeación didáctica por ID para el revisor.
    /// </summary>
    Task<PlaneacionDidacticaRevisorDto?> GetByIdForRevisorAsync(long id);

    /// <summary>
    /// Obtiene el detalle completo de una planeación para revisión.
    /// </summary>
    Task<(PlaneacionDetalleRevisionDto? Detalle, bool Exists, bool Authorized)> GetDetalleRevisionAsync(long id, long usuarioId);

    /// <summary>
    /// Registra una observación de revisor sobre la planeación o una unidad específica.
    /// </summary>
    Task<(PlaneacionObservacionDto? Observacion, bool Exists, bool Authorized, bool UnidadValid)> CrearObservacionAsync(
        long id,
        long usuarioId,
        CrearPlaneacionObservacionRequestDto request);

    /// <summary>
    /// Autoriza una planeación después de la evaluación manual/externa de criterios.
    /// </summary>
    Task<(PlaneacionEstadoDto? Planeacion, bool Exists, bool Authorized)> AutorizarAsync(long id, long usuarioId);

    /// <summary>
    /// Solicita revisión de una planeación y asegura su chat de revisión.
    /// </summary>
    Task<(PlaneacionRevisionSolicitadaDto? Planeacion, bool Exists, bool Authorized, bool HasRevisor, bool HasDocentes)> SolicitarRevisionAsync(long id, long usuarioId);

    /// <summary>
    /// Rechaza una planeación y la regresa a EnProceso con un motivo visible como observación.
    /// </summary>
    Task<(PlaneacionRechazoDto? Planeacion, bool Exists, bool Authorized)> RechazarAsync(
        long id,
        long usuarioId,
        PlaneacionRechazarRequestDto request);

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
