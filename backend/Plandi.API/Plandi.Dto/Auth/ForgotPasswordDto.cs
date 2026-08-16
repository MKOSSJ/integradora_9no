using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(128)]
        public string Email { get; set; } = string.Empty;
    }
}
