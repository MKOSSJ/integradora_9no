using System;
using System.Collections.Generic;
using System.Text;
using Plandi.Dto.Enums;


namespace Plandi.Library.Models
{
    public class AcademiaUsuario
    {
        public long AcademiaId { get; set; }
        public Academia Academia { get; set; } = null!;

        public long UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public RolAcademia RolEnAcademia { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
