using System.ComponentModel.DataAnnotations;

namespace Plandi.API.Models;

public class ImportarProgramasAsignaturaForm
{
    [Required]
    public List<IFormFile> Files { get; set; } = [];

    [Required]
    public Guid SubidoPorPublicId { get; set; }
}
