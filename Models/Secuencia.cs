using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace secuenciasAPI.Models
{
    public class Secuencia
    {
        public long Id { get; set; }
        public long PeriodoId { get; set; }
        public long MateriaId { get; set; }
        public long DirectorId { get; set; }
        public string? Descripcion { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public DateTime? FechaLimite { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public int Estado { get; set; }
        public string? Url { get; set; }
        public string? Contenido { get; set; }

        public Periodo? Periodo { get; set; }
        public Materia? Materia { get; set; }
        public User? Director { get; set; }
        public ICollection<SecuenciaDocente> SecuenciaDocentes { get; set; } = new List<SecuenciaDocente>();
        public ICollection<SecuenciaGrupo> SecuenciaGrupos { get; set; } = new List<SecuenciaGrupo>();
        public ICollection<RevisionSecuencia> Revisiones { get; set; } = new List<RevisionSecuencia>();
    }
}
