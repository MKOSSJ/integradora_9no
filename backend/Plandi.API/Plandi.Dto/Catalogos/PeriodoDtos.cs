using System;
using System.ComponentModel.DataAnnotations;
using Plandi.Dto.Enums;

namespace Plandi.Dto.Catalogos
{
    public class PeriodoRequestDto
    {
        [Required]
        public Guid CicloEscolarPublicId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public DateTime? FechaLimiteEntregaPlaneaciones { get; set; }
    }

    public class PeriodoResponseDto
    {
        public Guid PublicId { get; set; }

        public Guid CicloEscolarPublicId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public DateTime? FechaLimiteEntregaPlaneaciones { get; set; }

        public bool Activo { get; set; }

        public EstadoPeriodo Estado { get; set; }

        public EstadoPeriodo EstadoEfectivo { get; set; }

        public DateTime? FechaCierre { get; set; }

        public bool PermiteModificaciones { get; set; }
    }
}
