using Plandi.Dto.Enums;

namespace Plandi.Library.Models;

/// <summary>
/// Representa una etapa estructural de una unidad. No es una actividad: existe
/// incluso sin elementos de secuencia capturados por el docente.
/// </summary>
public class PlaneacionEtapaSecuencia : BaseEntity
{
    public long PlaneacionUnidadId { get; set; }
    public PlaneacionUnidad PlaneacionUnidad { get; set; } = null!;

    public FaseSecuencia Fase { get; set; }

    public ICollection<PlaneacionSecuencia> Elementos { get; set; } = new List<PlaneacionSecuencia>();
}
