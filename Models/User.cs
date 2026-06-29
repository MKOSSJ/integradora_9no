using System;
using System.Collections.Generic;

namespace secuenciasAPI.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string ApellidoPaterno { get; set; } = null!;
        public string? ApellidoMaterno { get; set; }
        public string? Username { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool Activo { get; set; } = true;
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public bool TwoFactorVerified { get; set; }

        public ICollection<CarreraDocente> CarreraDocentes { get; set; } = new List<CarreraDocente>();
        public ICollection<SecuenciaDocente> SecuenciaDocentes { get; set; } = new List<SecuenciaDocente>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<Secuencia> Secuencias { get; set; } = new List<Secuencia>();
        public ICollection<RevisionSecuencia> RevisionesSecuencia { get; set; } = new List<RevisionSecuencia>();
    }
}
