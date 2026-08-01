using System;
using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Catalogos
{
    public class CicloEscolarRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }
    }

    public class CicloEscolarResponseDto
    {
        public Guid PublicId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public bool Activo { get; set; }
    }
}
