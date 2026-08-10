using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionUnidad : BaseEntity
    {
        public long PlaneacionDidacticaId { get; set; }
        public PlaneacionDidactica PlaneacionDidactica { get; set; } = null!;

        public int? NumeroUnidad { get; set; } =null;

        public string NombreUnidad { get; set; } = string.Empty;

        public string? PropositoEsperado { get; set; }

        public int? HorasSaber { get; set; }

        public int? HorasSaberHacer { get; set; }

        public int? HorasTotales { get; set; }

        public decimal? PorcentajeUnidad { get; set; }

        public int Orden { get; set; }

        public long? UltimaModificacionPorId { get; set; }
        public Usuario? UltimaModificacionPor { get; set; }

        public DateTime? FechaUltimaModificacion { get; set; }

        // Child collections
        public ICollection<PlaneacionTema> Temas { get; set; } = new List<PlaneacionTema>();

        public ICollection<PlaneacionEvaluacion> Evaluaciones { get; set; } = new List<PlaneacionEvaluacion>();

        public ICollection<PlaneacionSecuencia> Secuencias { get; set; } = new List<PlaneacionSecuencia>();
    }
}
