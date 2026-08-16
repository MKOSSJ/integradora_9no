using System;
using System.ComponentModel.DataAnnotations;
using RolAcademia = Plandi.Dto.Enums.Rol;

namespace Plandi.Dto.Catalogos
{
    public class AcademiaUsuarioRequestDto
    {
        [Required]
        public Guid UsuarioPublicId { get; set; }

        [Required]
        [EnumDataType(typeof(RolAcademia))]
        public RolAcademia Rol { get; set; }
    }

    public class AcademiaUsuarioResponseDto
    {
        public Guid UsuarioPublicId { get; set; }

        public string? UsuarioNombre { get; set; }

        public RolAcademia Rol { get; set; }

        public bool Activo { get; set; }
    }
}
