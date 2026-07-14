using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionActividad : BaseEntity
    {
        public long PlaneacionUnidadId { get; set; }
        public PlaneacionUnidad PlaneacionUnidad { get; set; } = null!;

        public string TipoActividad { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int? Semana { get; set; }

        public int? Horas { get; set; }

        public string? EstrategiaEnsenanza { get; set; }

        public string? EstrategiaAprendizaje { get; set; }

        public string? Evidencia { get; set; }

        public string? InstrumentoEvaluacion { get; set; }

        public decimal? PorcentajeEvaluacion { get; set; }

        public int Orden { get; set; }

        public long? CreatedBy { get; set; }

        public long? UpdatedBy { get; set; }
    }
}
