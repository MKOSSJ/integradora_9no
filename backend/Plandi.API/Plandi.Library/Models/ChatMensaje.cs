using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class ChatMensaje : BaseEntity
    {
        public long ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        public long UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public string Mensaje { get; set; } = string.Empty;

        public string TipoMensaje { get; set; } = "TEXTO";

        public DateTime? EditadoAt { get; set; }

        public DateTime? EliminadoAt { get; set; }
    }
}
