namespace secuenciasAPI.Models
{
    public class SecuenciaDocente
    {
        public long SecuenciaId { get; set; }
        public long DocenteId { get; set; }

        public Secuencia? Secuencia { get; set; }
        public User? Docente { get; set; }
    }
}
