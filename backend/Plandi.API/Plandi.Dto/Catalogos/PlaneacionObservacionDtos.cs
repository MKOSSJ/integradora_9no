using System;
using System.Collections.Generic;

namespace Plandi.Dto
{
    // Response DTO (Output)
    public class PlaneacionObservacionDto
    {
        public long Id { get; set; }
        public long PlaneacionDidacticaId { get; set; }
        public long? PlaneacionUnidadId { get; set; }
        public long RevisorId { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public string Estado { get; set; } = "ABIERTA";
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; }
    }

    // Create DTO (Input)
    public class CreatePlaneacionObservacionDto
    {
        public long RevisorId { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public long? PlaneacionUnidadId { get; set; }
    }

    // Update DTO (Input)
    public class UpdatePlaneacionObservacionDto
    {
        public string? Comentario { get; set; }
        public string? Estado { get; set; }
    }
}
