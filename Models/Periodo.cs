using System.Collections.Generic;

namespace secuenciasAPI.Models
{
    public class Periodo
    {
        public long Id { get; set; }
        public short Anio { get; set; }
        public string? PeriodoTexto { get; set; }

        public ICollection<Secuencia> Secuencias { get; set; } = new List<Secuencia>();
    }
}
