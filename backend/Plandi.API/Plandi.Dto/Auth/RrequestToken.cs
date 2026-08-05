using System.ComponentModel.DataAnnotations;


namespace Plandi.Dto.Auth

{
    public class RequestToken
    {
        [Required]
        public required string RefreshToken { get; set; }
        public string? AccessToken { get; set; }
    }
}