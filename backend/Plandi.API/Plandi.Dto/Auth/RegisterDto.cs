using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Auth
{
    public class RegisterDTO
    {
        [Required]
        [MaxLength(64)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string? ApellidoMaterno { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(128)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(64)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare("Password")]
        [MaxLength(64)]
        public string? ConfirmPassword { get; set; }

        [Required]
        [MaxLength(20)]
        public string? Telefono { get; set; }
    }
}
