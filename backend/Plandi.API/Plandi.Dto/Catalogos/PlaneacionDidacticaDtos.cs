using Plandi.Dto.Enums;
using System;
using System.Collections.Generic;

namespace Plandi.Dto
{
    // Create DTO (Input)
    public class CreatePlaneacionDidacticaDto
    {
        public long PeriodoId { get; set; }
        public long AsignaturaId { get; set; }
        public long? AcademiaId { get; set; }
        public long? RevisorId { get; set; }
        public string Titulo { get; set; } = string.Empty;
    }

    // Update DTO (Input)
    public class UpdatePlaneacionDidacticaDto
    {
        public string? Titulo { get; set; }
        public EstadoPlaneacion? Estado { get; set; }
        public long? RevisorId { get; set; }
    }

    // Response DTO (Output) - with child collections
    public class PlaneacionDidacticaDto
    {
        public long Id { get; set; }
        public long PeriodoId { get; set; }
        public long AsignaturaId { get; set; }
        public long? AcademiaId { get; set; }
        public long? RevisorId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public EstadoPlaneacion Estado { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; }

        public PlaneacionCaratulaDto? Caratula { get; set; }
        public List<PlaneacionUnidadDto> Unidades { get; set; } = new();
        public List<PlaneacionReferenciaDto> Referencias { get; set; } = new();
        public List<PlaneacionObservacionDto> Observaciones { get; set; } = new();
    }

    // Simplified DTO for lists
    public class PlaneacionDidacticaSimpleDto
    {
        public long Id { get; set; }
        public long AsignaturaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public EstadoPlaneacion Estado { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}
