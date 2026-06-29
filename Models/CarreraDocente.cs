namespace secuenciasAPI.Models
{
    public class CarreraDocente
    {
        public long CarreraId { get; set; }
        public long DocenteId { get; set; }
        public bool Activo { get; set; } = true;

        public Carrera? Carrera { get; set; }
        public User? Docente { get; set; }
    }
}
