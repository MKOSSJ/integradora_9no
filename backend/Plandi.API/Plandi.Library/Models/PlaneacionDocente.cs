using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionDocente
    {
        public long PlaneacionDidacticaId { get; set; }
        public PlaneacionDidactica PlaneacionDidactica { get; set; } = null!;

        public long DocenteId { get; set; }
        public Usuario Docente { get; set; } = null!;

        public bool Activo { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
