using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class Academia : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public ICollection<Asignatura> Asignaturas { get; set; } = new List<Asignatura>();

        public ICollection<AcademiaUsuario> AcademiaUsuarios { get; set; } = new List<AcademiaUsuario>();

        public ICollection<CarreraAcademia> CarreraAcademias { get; set; } = new List<CarreraAcademia>();
    }
}
