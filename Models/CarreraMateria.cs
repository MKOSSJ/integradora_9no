namespace secuenciasAPI.Models
{
    public class CarreraMateria
    {
        public long CarreraId { get; set; }
        public long MateriaId { get; set; }

        public Carrera? Carrera { get; set; }
        public Materia? Materia { get; set; }
    }
}
