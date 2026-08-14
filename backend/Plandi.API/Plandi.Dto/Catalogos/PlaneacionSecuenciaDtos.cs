using Plandi.Dto.Enums;
using System;
using System.Collections.Generic;

namespace Plandi.Dto
{
    // Create DTO (Input)
    public class CreatePlaneacionSecuenciaDto
    {
        public FaseSecuencia Fase { get; set; }

        /// <summary>
        /// Estrategia entero correspondiente a la fase seleccionada.
        /// EstrategiaApertura, EstrategiaDesarrollo o EstrategiaCierre
        /// </summary>
        public int Estrategia { get; set; }

        public string? ActividadDocente { get; set; }
        public string? ActividadEstudiante { get; set; }
        public string? EvidenciaAprendizaje { get; set; }
        public string? MediosMateriales { get; set; }
        public int Orden { get; set; }
    }

    // Update DTO (Input)
    public class UpdatePlaneacionSecuenciaDto
    {
        public FaseSecuencia? Fase { get; set; }

        /// <summary>
        /// Estrategia entero correspondiente a la fase seleccionada.
        /// EstrategiaApertura, EstrategiaDesarrollo o EstrategiaCierre
        /// </summary>
        public int? Estrategia { get; set; }

        public string? ActividadDocente { get; set; }
        public string? ActividadEstudiante { get; set; }
        public string? EvidenciaAprendizaje { get; set; }
        public string? MediosMateriales { get; set; }
        public int? Orden { get; set; }
    }

    // Response DTO (Output)
    public class PlaneacionSecuenciaDto
    {
        public Guid PublicId { get; set; }
        public Guid PlaneacionUnidadPublicId { get; set; }
        public FaseSecuencia Fase { get; set; }

        /// <summary>
        /// Estrategia entero correspondiente a la fase seleccionada.
        /// EstrategiaApertura, EstrategiaDesarrollo o EstrategiaCierre
        /// </summary>
        public int Estrategia { get; set; }

        public string? ActividadDocente { get; set; }
        public string? ActividadEstudiante { get; set; }
        public string? EvidenciaAprendizaje { get; set; }
        public string? MediosMateriales { get; set; }
        public int Orden { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}
