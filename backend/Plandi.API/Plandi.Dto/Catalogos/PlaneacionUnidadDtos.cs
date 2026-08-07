using System;
using System.Collections.Generic;

namespace Plandi.Dto
{
    // Create DTO (Input)
    public class CreatePlaneacionUnidadDto
    {
        public string NumeroUnidad { get; set; } = string.Empty;
        public string NombreUnidad { get; set; } = string.Empty;
        public string? PropositoEsperado { get; set; }
        public int? HorasSaber { get; set; }
        public int? HorasSaberHacer { get; set; }
        public int? HorasTotales { get; set; }
        public decimal? PorcentajeUnidad { get; set; }
        public int Orden { get; set; }
    }

    // Update DTO (Input)
    public class UpdatePlaneacionUnidadDto
    {
        public string? NumeroUnidad { get; set; }
        public string? NombreUnidad { get; set; }
        public string? PropositoEsperado { get; set; }
        public int? HorasSaber { get; set; }
        public int? HorasSaberHacer { get; set; }
        public int? HorasTotales { get; set; }
        public decimal? PorcentajeUnidad { get; set; }
        public int? Orden { get; set; }
    }

    // Response DTO (Output) - with child collections
    public class PlaneacionUnidadDto
    {
        public long Id { get; set; }
        public long PlaneacionDidacticaId { get; set; }
        public string NumeroUnidad { get; set; } = string.Empty;
        public string NombreUnidad { get; set; } = string.Empty;
        public string? PropositoEsperado { get; set; }
        public int? HorasSaber { get; set; }
        public int? HorasSaberHacer { get; set; }
        public int? HorasTotales { get; set; }
        public decimal? PorcentajeUnidad { get; set; }
        public int Orden { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; }

        public List<PlaneacionTemaDto> Temas { get; set; } = new();
        public List<PlaneacionEvaluacionDto> Evaluaciones { get; set; } = new();
        public List<PlaneacionSecuenciaDto> Secuencias { get; set; } = new();
    }

    // Simplified DTO for lists
    public class PlaneacionUnidadSimpleDto
    {
        public long Id { get; set; }
        public string NumeroUnidad { get; set; } = string.Empty;
        public string NombreUnidad { get; set; } = string.Empty;
        public int Orden { get; set; }
        public bool Activo { get; set; }
    }
}
