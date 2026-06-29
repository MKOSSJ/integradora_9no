using System.Collections.Generic;

namespace secuenciasAPI.Models
{
    public class Carrera
    {
        public long Id { get; set; }
        public long? DirectorId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Status { get; set; } = true;

        public User? Director { get; set; }
        public ICollection<CarreraDocente> CarreraDocentes { get; set; } = new List<CarreraDocente>();
        public ICollection<CarreraMateria> CarreraMaterias { get; set; } = new List<CarreraMateria>();
        public ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();
    }
}
