using Plandi.Dto.Enums;
using System;
using System.Collections.Generic;

namespace Plandi.Dto
{
    // Create DTO (Input)
    public class CreatePlaneacionCaratulaDto
    {
        public long? ProgramaAsignaturaId { get; set; }
        public string? ProgramaEducativo { get; set; }
        public int? Cuatrimestre { get; set; }
        public string? NombreAsignatura { get; set; }
        public string? Docentes { get; set; }
        public string? PeriodoEscolar { get; set; }
        public string? Grupos { get; set; }
        public string? PropositoAsignatura { get; set; }
        public string? CompetenciaAsignatura { get; set; }
        public string? TipoCompetencia { get; set; }
        public decimal? Creditos { get; set; }
        public string? Modalidad { get; set; }
        public int? HorasSaber { get; set; }
        public int? HorasSaberHacer { get; set; }
        public int? HorasTotales { get; set; }
        public int? HorasSemana { get; set; }
    }

    // Update DTO (Input)
    public class UpdatePlaneacionCaratulaDto
    {
        public long? ProgramaAsignaturaId { get; set; }
        public string? ProgramaEducativo { get; set; }
        public int? Cuatrimestre { get; set; }
        public string? NombreAsignatura { get; set; }
        public string? Docentes { get; set; }
        public string? PeriodoEscolar { get; set; }
        public string? Grupos { get; set; }
        public string? PropositoAsignatura { get; set; }
        public string? CompetenciaAsignatura { get; set; }
        public string? TipoCompetencia { get; set; }
        public decimal? Creditos { get; set; }
        public string? Modalidad { get; set; }
        public int? HorasSaber { get; set; }
        public int? HorasSaberHacer { get; set; }
        public int? HorasTotales { get; set; }
        public int? HorasSemana { get; set; }
    }

    // Response DTO (Output)
    public class PlaneacionCaratulaDto
    {
        public long Id { get; set; }
        public long PlaneacionDidacticaId { get; set; }
        public long? ProgramaAsignaturaId { get; set; }
        public string? ProgramaEducativo { get; set; }
        public int? Cuatrimestre { get; set; }
        public string? NombreAsignatura { get; set; }
        public string? Docentes { get; set; }
        public string? PeriodoEscolar { get; set; }
        public string? Grupos { get; set; }
        public string? PropositoAsignatura { get; set; }
        public string? CompetenciaAsignatura { get; set; }
        public string? TipoCompetencia { get; set; }
        public decimal? Creditos { get; set; }
        public string? Modalidad { get; set; }
        public int? HorasSaber { get; set; }
        public int? HorasSaberHacer { get; set; }
        public int? HorasTotales { get; set; }
        public int? HorasSemana { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}
