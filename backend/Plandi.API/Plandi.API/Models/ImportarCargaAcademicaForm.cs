using System.ComponentModel.DataAnnotations;

namespace Plandi.API.Models;

public class ImportarCargaAcademicaForm
{
    [Required]
    public IFormFile? File { get; set; }

    [Required]
    public Guid PeriodoPublicId { get; set; }
}
