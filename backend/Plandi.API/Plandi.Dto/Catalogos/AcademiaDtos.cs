using System;
using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Catalogos
{
    public class AcademiaRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }
    }

    public class AcademiaResponseDto
    {
        public Guid PublicId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}
