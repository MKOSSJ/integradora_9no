using Plandi.Dto.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionEvaluacion : BaseEntity
    {
        public long PlaneacionUnidadId { get; set; }
        public PlaneacionUnidad PlaneacionUnidad { get; set; } = null!;

        public int? PeriodoSemanas { get; set; }

        public string? ResultadoAprendizaje { get; set; }

        public string? EvidenciaAprendizaje { get; set; }

        public FaseSecuencia Fase { get; set; }

        /// <summary>
        /// Tipo de evaluación según clasificación UTH
        /// </summary>
        public TipoEvaluacion? TipoEvaluacion { get; set; }

        public AgenteEvaluador AgenteEvaluador { get; set; }

        public decimal? Ponderacion { get; set; }

        public string? InstrumentoEvaluacion { get; set; }

        public int Orden { get; set; }

        public long? UltimaModificacionPorId { get; set; }
        public Usuario? UltimaModificacionPor { get; set; }

        public DateTime? FechaUltimaModificacion { get; set; }
    }
}
