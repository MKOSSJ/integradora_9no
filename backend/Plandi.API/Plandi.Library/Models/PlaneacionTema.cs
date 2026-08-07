using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionTema : BaseEntity
    {
        public long PlaneacionUnidadId { get; set; }
        public PlaneacionUnidad PlaneacionUnidad { get; set; } = null!;

        public string Tema { get; set; } = string.Empty;

        public string? SaberConceptual { get; set; }

        public string? SaberHacer { get; set; }

        public string? SaberSer { get; set; }

        public int Orden { get; set; }

        public long? UltimaModificacionPorId { get; set; }
        public Usuario? UltimaModificacionPor { get; set; }

        public DateTime? FechaUltimaModificacion { get; set; }
    }
}
