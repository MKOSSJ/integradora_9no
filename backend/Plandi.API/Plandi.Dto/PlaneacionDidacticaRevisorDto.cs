namespace Plandi.Dto;
using Plandi.Dto.Enums;

public class PlaneacionDidacticaRevisorDto
{
    public string AsignaturaNombre { get; set; } = string.Empty;
    public string PeriodoNombre { get; set; } = string.Empty;
    public string PeriodoFechaInicio { get; set; } = string.Empty;
    public string PeriodoFechaFin { get; set; } = string.Empty;
    public string AcademiaNombre { get; set; } = string.Empty;
    public int ProgramaAsignaturaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public EstadoPlaneacion Estado { get; set; }
    public string FechaUltimaModificacion { get; set; } = string.Empty;
    
    public int UsuarioUltimaModificacion { get; set; }
}
