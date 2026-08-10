
namespace Plandi.Library.Models
{
    public class RefreshToken : BaseEntity
    {
        public long UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public string TokenHash { get; set; } = string.Empty;

        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;

        public DateTime Created { get; set; }

        public DateTime? Revoked { get; set; }

        public string? ReplacedByTokenHash { get; set; }

        public bool IsRevoked => Revoked != null;
        public bool isActive => !IsExpired && !IsRevoked;
    }
}