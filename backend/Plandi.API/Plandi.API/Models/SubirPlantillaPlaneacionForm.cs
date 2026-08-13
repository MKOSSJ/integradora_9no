using System.ComponentModel.DataAnnotations;

namespace Plandi.API.Models;

public sealed class SubirPlantillaPlaneacionForm
{
    [Required]
    public IFormFile? File { get; set; }
}
