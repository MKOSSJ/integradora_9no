using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class Usuario : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public string ApellidoPaterno { get; set; } = string.Empty;

        public string? ApellidoMaterno { get; set; }

        public string? Email { get; set; }

        public string? PasswordHash { get; set; }

        public string? Telefono { get; set; }

        public DateTime? UltimoAcceso { get; set; }

        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecretKey { get; set; }   

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }

        public int AccessFailedCount { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();

        public ICollection<AcademiaUsuario> AcademiaUsuarios { get; set; } = new List<AcademiaUsuario>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public ICollection<UserDeviceToken> UserDeviceTokens { get; set; } = new List<UserDeviceToken>();
    }
}
