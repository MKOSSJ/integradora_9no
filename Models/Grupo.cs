using System.Collections.Generic;

namespace secuenciasAPI.Models
{
    public class Grupo
    {
        public long Id { get; set; }
        public long CarreraId { get; set; }
        public string Nombre { get; set; } = null!;
        public byte Cuatrimestre { get; set; }
        public bool Activo { get; set; } = true;

        public Carrera? Carrera { get; set; }
        public ICollection<SecuenciaGrupo> SecuenciaGrupos { get; set; } = new List<SecuenciaGrupo>();
    }
}
