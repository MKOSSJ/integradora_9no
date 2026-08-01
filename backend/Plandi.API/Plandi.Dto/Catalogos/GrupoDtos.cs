using System;
using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Catalogos
{
    public class GrupoRequestDto
    {
        [Required]
        public Guid CarreraPublicId { get; set; }

        [Required]
        public Guid PeriodoPublicId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Cuatrimestre { get; set; }
    }

    public class GrupoResponseDto
    {
        public Guid PublicId { get; set; }

        public Guid CarreraPublicId { get; set; }

        public Guid PeriodoPublicId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int Cuatrimestre { get; set; }

        public bool Activo { get; set; }
    }
}
