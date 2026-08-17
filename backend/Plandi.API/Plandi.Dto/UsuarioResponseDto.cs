using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Dto
{
    public class UsuarioResponseDto
    {
        public Guid PublicId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string ApellidoPaterno { get; set; } = string.Empty;

        public string? ApellidoMaterno { get; set; }

        public string? Email { get; set; }

        public string? Telefono { get; set; }

        public DateTime? UltimoAcceso { get; set; }

        public bool Activo { get; set; }

        public bool CredencialesCompletas { get; set; }
    }
}
