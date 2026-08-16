namespace Plandi.Library.Models;

/// <summary>Recurso o medio didáctico de un elemento específico de secuencia.</summary>
public class PlaneacionSecuenciaRecurso : BaseEntity
{
    public long PlaneacionSecuenciaId { get; set; }
    public PlaneacionSecuencia PlaneacionSecuencia { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
}
