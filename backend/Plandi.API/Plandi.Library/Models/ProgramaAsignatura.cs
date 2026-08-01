using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class ProgramaAsignatura : BaseEntity
    {
        public long DocumentoId { get; set; }
        public Documento Documento { get; set; } = null!;

        public long? AsignaturaId { get; set; }
        public Asignatura? Asignatura { get; set; }

        public long? AcademiaId { get; set; }
        public Academia? Academia { get; set; }

        public string NombreAsignatura { get; set; } = string.Empty;

        public string? ClaveAsignatura { get; set; }

        public string? Carrera { get; set; }

        public int? Cuatrimestre { get; set; }

        public string? Competencia { get; set; }

        public string? Proposito { get; set; }

        public decimal? Creditos { get; set; }

        public int? HorasTotales { get; set; }

        public int? HorasSemana { get; set; }

        public string? TextoExtraido { get; set; }

        public string? JsonExtraido { get; set; }

        public long? UltimaModificacionPorId { get; set; }
        public Usuario? UltimaModificacionPor { get; set; }

        public DateTime? FechaUltimaModificacion { get; set; }
    }
}
