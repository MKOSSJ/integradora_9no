using System.Text.Json.Nodes;

namespace secuenciasAPI.Models
{
    public class Programa
    {
        public long Id { get; set; }
        public long IdMateria { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }

        public string? Url { get; set; }
        public string? Contenido { get; set; }
        public bool Activo { get; set; } = true;

        public Materia? Materia { get; set; }
    }
}
