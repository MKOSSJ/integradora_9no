namespace secuenciasAPI.Models
{
    public class SecuenciaGrupo
    {
        public long SecuenciaId { get; set; }
        public long GrupoId { get; set; }

        public Secuencia? Secuencia { get; set; }
        public Grupo? Grupo { get; set; }
    }
}
