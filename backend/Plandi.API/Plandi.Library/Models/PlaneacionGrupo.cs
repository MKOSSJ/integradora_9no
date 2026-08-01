using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionGrupo
    {
        public long PlaneacionDidacticaId { get; set; }
        public PlaneacionDidactica PlaneacionDidactica { get; set; } = null!;

        public long GrupoId { get; set; }
        public Grupo Grupo { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
