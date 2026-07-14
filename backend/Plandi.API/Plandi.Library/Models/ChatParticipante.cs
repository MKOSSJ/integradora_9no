using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class ChatParticipante
    {
        public long ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        public long UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public string RolEnChat { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
