using System;
using System.Collections.Generic;

namespace Plandi.Dto
{
    // Create DTO (Input)
    public class CreatePlaneacionTemaDtos
    {
        public string Tema { get; set; } = string.Empty;
        public string? SaberConceptual { get; set; }
        public string? SaberHacer { get; set; }
        public string? SaberSer { get; set; }
        public int Orden { get; set; }
    }

    // Update DTO (Input)
    public class UpdatePlaneacionTemaDtos
    {
        public string? Tema { get; set; }
        public string? SaberConceptual { get; set; }
        public string? SaberHacer { get; set; }
        public string? SaberSer { get; set; }
        public int? Orden { get; set; }
    }

    // Response DTO (Output)
    public class PlaneacionTemaDto
    {
        public long Id { get; set; }
        public long PlaneacionUnidadId { get; set; }
        public string Tema { get; set; } = string.Empty;
        public string? SaberConceptual { get; set; }
        public string? SaberHacer { get; set; }
        public string? SaberSer { get; set; }
        public int Orden { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}
