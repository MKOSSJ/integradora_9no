using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Auth
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        [MaxLength(128)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(64)]
        public string Password { get; set; } = string.Empty;
    }
}