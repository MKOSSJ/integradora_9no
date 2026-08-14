using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class CicloEscolar : BaseEntity
    {
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public ICollection<Periodo> Periodos { get; set; } = new List<Periodo>();
    }
}
