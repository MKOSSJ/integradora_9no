using System.ComponentModel.DataAnnotations;


namespace Plandi.Dto.Auth

{
    public class TwoFactorDto
    {
        [Required]
        public required string Code { get; set; }

        [Required]  
        public required string email { get; set; }
    }
}