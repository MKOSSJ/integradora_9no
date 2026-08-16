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

        public long PlaneacionEtapaSecuenciaId { get; set; }
        public PlaneacionEtapaSecuencia EtapaSecuencia { get; set; } = null!;

        // Se conserva para compatibilidad con los registros y endpoints
        // heredados. La fase canónica es la de EtapaSecuencia.
        public FaseSecuencia Fase { get; set; }

        /// <summary>
        /// Estrategia asociada a la fase.
        /// Almacena el valor entero del enum correspondiente a la fase
        /// (EstrategiaApertura, EstrategiaDesarrollo o EstrategiaCierre).
        /// Validación en servicio garantiza correspondencia con Fase.
        /// </summary>
        public int Estrategia { get; set; }

        /// <summary>
        /// Método o técnica controlado por el sistema. Sustituye la ambigüedad
        /// de interpretar Estrategia con un enum distinto según la fase.
        /// </summary>
        public MetodoTecnicaEnsenanzaAprendizaje? MetodoTecnica { get; set; }

        public string? ActividadDocente { get; set; }

        public string? ActividadEstudiante { get; set; }

        public string? EvidenciaAprendizaje { get; set; }

        public string? MediosMateriales { get; set; }

        public int Orden { get; set; }

        public long? UltimaModificacionPorId { get; set; }
        public Usuario? UltimaModificacionPor { get; set; }

        public DateTime? FechaUltimaModificacion { get; set; }

        public ICollection<PlaneacionSecuenciaRecurso> Recursos { get; set; } = new List<PlaneacionSecuenciaRecurso>();
    }
}
