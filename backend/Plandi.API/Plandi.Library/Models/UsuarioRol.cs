using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class UsuarioRol
    {
        public long UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public long RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
