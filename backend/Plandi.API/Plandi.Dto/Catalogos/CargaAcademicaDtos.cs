using System;
using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Catalogos
{
    public class CargaAcademicaRequestDto
    {
        [Required]
        public Guid PeriodoPublicId { get; set; }

        [Required]
        public Guid GrupoPublicId { get; set; }

        [Required]
        public Guid AsignaturaPublicId { get; set; }

        [Required]
        public Guid DocentePublicId { get; set; }

        public Guid? RevisorPublicId { get; set; }

        public Guid? AcademiaPublicId { get; set; }
    }

    public class CargaAcademicaResponseDto
    {
        public Guid PublicId { get; set; }

        public Guid PeriodoPublicId { get; set; }

        public Guid GrupoPublicId { get; set; }

        public Guid AsignaturaPublicId { get; set; }

        public Guid DocentePublicId { get; set; }

        public Guid? RevisorPublicId { get; set; }

        public Guid? AcademiaPublicId { get; set; }

        public bool Activo { get; set; }
    }
}
