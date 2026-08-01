using System;
using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Catalogos
{
    public class AsignaturaRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Clave { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Cuatrimestre { get; set; }

        [Range(1, int.MaxValue)]
        public int HorasTotales { get; set; }

        [Range(1, int.MaxValue)]
        public int HorasSemana { get; set; }

        [Range(0, 999.99)]
        public decimal Creditos { get; set; }

        public Guid? AcademiaPublicId { get; set; }
    }

    public class AsignaturaResponseDto
    {
        public Guid PublicId { get; set; }

        public Guid? AcademiaPublicId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Clave { get; set; } = string.Empty;

        public int Cuatrimestre { get; set; }

        public int HorasTotales { get; set; }

        public int HorasSemana { get; set; }

        public decimal Creditos { get; set; }

        public bool Activo { get; set; }
    }
}
