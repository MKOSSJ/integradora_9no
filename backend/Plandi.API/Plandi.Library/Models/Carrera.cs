using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class Carrera : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public string Clave { get; set; } = string.Empty;

        public string? Nivel { get; set; }

        public ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();

        public ICollection<CarreraAcademia> CarreraAcademias { get; set; } = new List<CarreraAcademia>();
    }
}
