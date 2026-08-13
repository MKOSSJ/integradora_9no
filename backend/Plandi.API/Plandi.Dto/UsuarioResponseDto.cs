using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Dto
{
    public class UsuarioResponseDto
    {
        public string Nombre { get; set; }

        public string ApellidoPaterno { get; set; } 

        public string? ApellidoMaterno { get; set; }

        public string? Email { get; set; }

        public string? Telefono { get; set; }

        public DateTime? UltimoAcceso { get; set; }
    }
}
