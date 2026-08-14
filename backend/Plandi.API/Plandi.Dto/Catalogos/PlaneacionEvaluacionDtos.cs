using Plandi.Dto.Enums;
using System;
using System.Collections.Generic;

namespace Plandi.Dto
{
    // Create DTO (Input)
    public class CreatePlaneacionEvaluacionDto
    {
        public string? PeriodoSemanas { get; set; }
        public string? ResultadoAprendizaje { get; set; }
        public string? EvidenciaAprendizaje { get; set; }
        public FaseSecuencia Fase { get; set; }
        public TipoEvaluacion? TipoEvaluacion { get; set; }
        public AgenteEvaluador AgenteEvaluador { get; set; }
        public decimal? Ponderacion { get; set; }
        public string? InstrumentoEvaluacion { get; set; }
        public int Orden { get; set; }
    }

    // Update DTO (Input)
    public class UpdatePlaneacionEvaluacionDto
    {
        public string? PeriodoSemanas { get; set; }
        public string? ResultadoAprendizaje { get; set; }
        public string? EvidenciaAprendizaje { get; set; }
        public FaseSecuencia? Fase { get; set; }
        public TipoEvaluacion? TipoEvaluacion { get; set; }
        public AgenteEvaluador? AgenteEvaluador { get; set; }
        public decimal? Ponderacion { get; set; }
        public string? InstrumentoEvaluacion { get; set; }
        public int? Orden { get; set; }
    }

    // Response DTO (Output)
    public class PlaneacionEvaluacionDto
    {
        public Guid PublicId { get; set; }
        public Guid PlaneacionUnidadPublicId { get; set; }
        public string? PeriodoSemanas { get; set; }
        public string? ResultadoAprendizaje { get; set; }
        public string? EvidenciaAprendizaje { get; set; }
        public FaseSecuencia Fase { get; set; }
        public TipoEvaluacion? TipoEvaluacion { get; set; }
        public AgenteEvaluador AgenteEvaluador { get; set; }
        public decimal? Ponderacion { get; set; }
        public string? InstrumentoEvaluacion { get; set; }
        public int Orden { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}
