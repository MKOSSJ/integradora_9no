using System.Collections.Generic;

namespace secuenciasAPI.Models
{
    public class Materia
    {
        public long Id { get; set; }
        public string Clave { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public byte Cuatrimestre { get; set; }

        public ICollection<CarreraMateria> CarreraMaterias { get; set; } = new List<CarreraMateria>();
        public ICollection<Programa> Programas { get; set; } = new List<Programa>();
        public ICollection<Secuencia> Secuencias { get; set; } = new List<Secuencia>();
    }
}
