using Plandi.Dto.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto
{
    // Create DTO (Input)
    public class CreatePlaneacionCaratulaDto
    {
        public Guid? ProgramaAsignaturaPublicId { get; set; }
        [MaxLength(200)]
        public string? ProgramaEducativo { get; set; }
        [Range(1, int.MaxValue)]
        public int? Cuatrimestre { get; set; }
        [MaxLength(200)]
        public string? NombreAsignatura { get; set; }
        public string? Docentes { get; set; }
        [MaxLength(100)]
        public string? PeriodoEscolar { get; set; }
        [MaxLength(500)]
        public string? Grupos { get; set; }
        public string? PropositoAsignatura { get; set; }
        public string? CompetenciaAsignatura { get; set; }
        [MaxLength(100)]
        public string? TipoCompetencia { get; set; }
        [Range(typeof(decimal), "0", "999.99")]
        public decimal? Creditos { get; set; }
        [MaxLength(100)]
        public string? Modalidad { get; set; }
        [Range(0, int.MaxValue)]
        public int? HorasSaber { get; set; }
        [Range(0, int.MaxValue)]
        public int? HorasSaberHacer { get; set; }
        [Range(0, int.MaxValue)]
        public int? HorasTotales { get; set; }
        [Range(0, int.MaxValue)]
        public int? HorasSemana { get; set; }
    }

    // Update DTO (Input)
    public class UpdatePlaneacionCaratulaDto
    {
        public Guid? ProgramaAsignaturaPublicId { get; set; }
        [MaxLength(200)]
        public string? ProgramaEducativo { get; set; }
        [Range(1, int.MaxValue)]
        public int? Cuatrimestre { get; set; }
        [MaxLength(200)]
        public string? NombreAsignatura { get; set; }
        public string? Docentes { get; set; }
        [MaxLength(100)]
        public string? PeriodoEscolar { get; set; }
        [MaxLength(500)]
        public string? Grupos { get; set; }
        public string? PropositoAsignatura { get; set; }
        public string? CompetenciaAsignatura { get; set; }
        [MaxLength(100)]
        public string? TipoCompetencia { get; set; }
        [Range(typeof(decimal), "0", "999.99")]
        public decimal? Creditos { get; set; }
        [MaxLength(100)]
        public string? Modalidad { get; set; }
        [Range(0, int.MaxValue)]
        public int? HorasSaber { get; set; }
        [Range(0, int.MaxValue)]
        public int? HorasSaberHacer { get; set; }
        [Range(0, int.MaxValue)]
        public int? HorasTotales { get; set; }
        [Range(0, int.MaxValue)]
        public int? HorasSemana { get; set; }
    }

    // Response DTO (Output)
    public class PlaneacionCaratulaDto
    {
        public Guid PublicId { get; set; }
        public Guid PlaneacionDidacticaPublicId { get; set; }
        public Guid? ProgramaAsignaturaPublicId { get; set; }
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
