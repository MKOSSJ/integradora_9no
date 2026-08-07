using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionCaratula : BaseEntity
    {
        public long PlaneacionDidacticaId { get; set; }
        public PlaneacionDidactica PlaneacionDidactica { get; set; } = null!;

        public long? ProgramaAsignaturaId { get; set; }
        public ProgramaAsignatura? ProgramaAsignatura { get; set; }

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

        public long? UltimaModificacionPorId { get; set; }
        public Usuario? UltimaModificacionPor { get; set; }

        public int Version { get; set; } = 1;
    }
}
