using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Auth
{
    public class ResetPasswordDto
    {
        [Required]
        public string PasswordResetToken { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(64)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
