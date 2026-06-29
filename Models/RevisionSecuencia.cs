using System;

namespace secuenciasAPI.Models
{
    public class RevisionSecuencia
    {
        public long Id { get; set; }
        public long SecuenciaId { get; set; }
        public long RevisorId { get; set; }
        public int Resultado { get; set; }
        public string? Comentarios { get; set; }
        public DateTime FechaRevision { get; set; }

        public Secuencia? Secuencia { get; set; }
        public User? Revisor { get; set; }
    }
}
