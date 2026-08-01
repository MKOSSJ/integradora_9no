using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class Usuario : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public string ApellidoPaterno { get; set; } = string.Empty;

        public string? ApellidoMaterno { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public DateTime? UltimoAcceso { get; set; }

        public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();

        public ICollection<AcademiaUsuario> AcademiaUsuarios { get; set; } = new List<AcademiaUsuario>();
    }
}
