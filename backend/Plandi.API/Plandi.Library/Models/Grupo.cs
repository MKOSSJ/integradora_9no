using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class Grupo : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public int Cuatrimestre { get; set; }

        public long CarreraId { get; set; }
        public Carrera Carrera { get; set; } = null!;

        public long PeriodoId { get; set; }
        public Periodo Periodo { get; set; } = null!;

        public ICollection<CargaAcademica> CargasAcademicas { get; set; } = new List<CargaAcademica>();

        public ICollection<PlaneacionGrupo> PlaneacionGrupos { get; set; } = new List<PlaneacionGrupo>();
    }
}
