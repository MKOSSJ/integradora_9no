using System.Collections.Generic;

namespace secuenciasAPI.Models
{
    public class Role
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; } = true;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
