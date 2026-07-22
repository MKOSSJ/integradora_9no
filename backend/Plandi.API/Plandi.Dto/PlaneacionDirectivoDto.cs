namespace Plandi.Dto;

public class PlaneacionDirectivoDto : PlaneacionDidacticaRevisorDto
{
    public string NombreRevisor { get; set; } = string.Empty;
    public int IdRevisor { get; set; }
    public string NombreMaestro { get; set; } = string.Empty;
    public int IdMaestro { get; set; }
    public string NombreUltimoModificacion { get; set; } = string.Empty;
    public int IdUltimoModificacion { get; set; }
}