using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto
{
    // Create DTO (Input)
    public class CreatePlaneacionTemaDtos
    {
        [Required]
        [MaxLength(250)]
        public string Tema { get; set; } = string.Empty;
        public string? SaberConceptual { get; set; }
        public string? SaberHacer { get; set; }
        public string? SaberSer { get; set; }
        [Range(0, int.MaxValue)]
        public int Orden { get; set; }
    }

    // Update DTO (Input)
    public class UpdatePlaneacionTemaDtos
    {
        [MaxLength(250)]
        public string? Tema { get; set; }
        public string? SaberConceptual { get; set; }
        public string? SaberHacer { get; set; }
        public string? SaberSer { get; set; }
        [Range(0, int.MaxValue)]
        public int? Orden { get; set; }
    }

    // Response DTO (Output)
    public class PlaneacionTemaDto
    {
        public Guid PublicId { get; set; }
        public Guid PlaneacionUnidadPublicId { get; set; }
        public string Tema { get; set; } = string.Empty;
        public string? SaberConceptual { get; set; }
        public string? SaberHacer { get; set; }
        public string? SaberSer { get; set; }
        public int Orden { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}
