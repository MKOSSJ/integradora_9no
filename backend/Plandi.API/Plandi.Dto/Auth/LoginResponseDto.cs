using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Auth
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; } = string.Empty;
        public DateTime? AccessTokenExpiresAt { get; set; }
        public string? RefreshToken { get; set; } = string.Empty;
        public bool RequiresTwoFactor { get; set; } = false; 
    }   
}