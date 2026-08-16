namespace Plandi.Library.Models;

public class CarreraAcademia
{
    public long CarreraId { get; set; }
    public Carrera Carrera { get; set; } = null!;

    public long AcademiaId { get; set; }
    public Academia Academia { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
