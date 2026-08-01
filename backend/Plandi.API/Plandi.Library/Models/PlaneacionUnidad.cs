using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionUnidad : BaseEntity
    {
        public long PlaneacionDidacticaId { get; set; }
        public PlaneacionDidactica PlaneacionDidactica { get; set; } = null!;

        public string Numero { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? ResultadoAprendizaje { get; set; }

        public int? Horas { get; set; }

        public int Orden { get; set; }

        public long? UltimaModificacionPorId { get; set; }
        public Usuario? UltimaModificacionPor { get; set; }

        public DateTime? FechaUltimaModificacion { get; set; }

        public ICollection<PlaneacionActividad> Actividades { get; set; } = new List<PlaneacionActividad>();
    }
}
