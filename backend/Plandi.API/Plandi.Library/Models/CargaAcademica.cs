using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class CargaAcademica : BaseEntity
    {
        public long PeriodoId { get; set; }
        public Periodo Periodo { get; set; } = null!;

        public long GrupoId { get; set; }
        public Grupo Grupo { get; set; } = null!;

        public long AsignaturaId { get; set; }
        public Asignatura Asignatura { get; set; } = null!;

        public long DocenteId { get; set; }
        public Usuario Docente { get; set; } = null!;

        public long? RevisorId { get; set; }
        public Usuario? Revisor { get; set; }

        public long? AcademiaId { get; set; }
        public Academia? Academia { get; set; }

        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
    }
}
