using System;
using System.ComponentModel.DataAnnotations;
using Plandi.Dto.Enums;

namespace Plandi.Dto.Catalogos
{
    public class AcademiaUsuarioRequestDto
    {
        [Required]
        public Guid UsuarioPublicId { get; set; }

        [Required]
        public RolAcademia RolEnAcademia { get; set; }
    }

    public class AcademiaUsuarioResponseDto
    {
        public Guid UsuarioPublicId { get; set; }

        public string? UsuarioNombre { get; set; }

        public RolAcademia RolEnAcademia { get; set; }

        public bool Activo { get; set; }
    }
}
