using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class Academia : BaseEntity
    {
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public ICollection<Asignatura> Asignaturas { get; set; } = new List<Asignatura>();

        public ICollection<AcademiaUsuario> AcademiaUsuarios { get; set; } = new List<AcademiaUsuario>();
    }
}
