using Plandi.Dto.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionDidactica : BaseEntity
    {
        public long PeriodoId { get; set; }
        public Periodo Periodo { get; set; } = null!;

        public long AsignaturaId { get; set; }
        public Asignatura Asignatura { get; set; } = null!;

        public long? AcademiaId { get; set; }
        public Academia? Academia { get; set; }

        public long? ProgramaAsignaturaId { get; set; }
        public ProgramaAsignatura? ProgramaAsignatura { get; set; }

        public long? RevisorId { get; set; }
        public Usuario? Revisor { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public EstadoPlaneacion Estado { get; set; } = EstadoPlaneacion.Borrador;

        public long? UltimaModificacionPorId { get; set; }
        public Usuario? UltimaModificacionPor { get; set; }

        public DateTime? FechaUltimaModificacion { get; set; }

        public long? CreatedBy { get; set; }

        public ICollection<PlaneacionDocente> PlaneacionDocentes { get; set; } = new List<PlaneacionDocente>();

        public ICollection<PlaneacionGrupo> PlaneacionGrupos { get; set; } = new List<PlaneacionGrupo>();

        public ICollection<PlaneacionUnidad> Unidades { get; set; } = new List<PlaneacionUnidad>();

        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
    }
}
