using System;
using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Catalogos
{
    public class CarreraRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Clave { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Nivel { get; set; }
    }

    public class CarreraResponseDto
    {
        public Guid PublicId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Clave { get; set; } = string.Empty;

        public string? Nivel { get; set; }

        public bool Activo { get; set; }
    }
}
