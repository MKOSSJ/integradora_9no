using Plandi.Dto.Enums;

namespace Plandi.Dto;

/// <summary>
/// Filtros para la búsqueda de planeaciones didácticas.
/// Se usa como [FromQuery] en GET /api/PlaneacionDidactica.
/// Todos los filtros son opcionales — solo se aplican los que tienen valor.
/// </summary>
public class PlaneacionFilterDto
{
    /// <summary>Filtra por carrera (vía CarreraAcademia → Academia → Planeacion)</summary>
    public long? CarreraId { get; init; }

    /// <summary>Filtra por periodo académico</summary>
    public long? PeriodoId { get; init; }

    /// <summary>Filtra por asignatura</summary>
    public long? AsignaturaId { get; init; }

    /// <summary>Filtra por docente (vía PlaneacionDocentes)</summary>
    public long? DocenteId { get; init; }

    /// <summary>Filtra por fecha de última modificación (desde)</summary>
    public DateTime? FechaDesde { get; init; }

    /// <summary>Filtra por fecha de última modificación (hasta)</summary>
    public DateTime? FechaHasta { get; init; }

    /// <summary>Filtra por estado de la planeación</summary>
    public EstadoPlaneacion? Estado { get; init; }
}
