using System;
using System.Collections.Generic;

namespace Plandi.Dto
{
    // Create DTO (Input)
    public class CreatePlaneacionReferenciaDto
    {
        public string ReferenciaAPA { get; set; } = string.Empty;
        public int Orden { get; set; }
    }

    // Update DTO (Input)
    public class UpdatePlaneacionReferenciaDto
    {
        public string? ReferenciaAPA { get; set; }
        public int? Orden { get; set; }
    }

    // Response DTO (Output)
    public class PlaneacionReferenciaDto
    {
        public long Id { get; set; }
        public long PlaneacionDidacticaId { get; set; }
        public string ReferenciaAPA { get; set; } = string.Empty;
        public int Orden { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}
