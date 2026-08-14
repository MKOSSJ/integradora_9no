using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Catalogos;

public sealed class CompletarCredencialesDocenteDto
{
    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(64)]
    public string Password { get; set; } = string.Empty;
}

public sealed class CredencialesDocenteDto
{
    public Guid UsuarioPublicId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool CredencialesCompletas { get; set; }
}
