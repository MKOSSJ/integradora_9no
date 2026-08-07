using Plandi.Dto.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionSecuencia : BaseEntity
    {
        public long PlaneacionUnidadId { get; set; }
        public PlaneacionUnidad PlaneacionUnidad { get; set; } = null!;

        public FaseSecuencia Fase { get; set; }

        /// <summary>
        /// Estrategia asociada a la fase.
        /// Almacena el valor entero del enum correspondiente a la fase
        /// (EstrategiaApertura, EstrategiaDesarrollo o EstrategiaCierre).
        /// Validación en servicio garantiza correspondencia con Fase.
        /// </summary>
        public int Estrategia { get; set; }

        public string? ActividadDocente { get; set; }

        public string? ActividadEstudiante { get; set; }

        public string? EvidenciaAprendizaje { get; set; }

        public string? MediosMateriales { get; set; }

        public int Orden { get; set; }

        public long? UltimaModificacionPorId { get; set; }
        public Usuario? UltimaModificacionPor { get; set; }

        public DateTime? FechaUltimaModificacion { get; set; }
    }
}
