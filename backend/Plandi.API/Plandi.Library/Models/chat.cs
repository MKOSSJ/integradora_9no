using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class Chat : BaseEntity
    {
        public long PlaneacionDidacticaId { get; set; }
        public PlaneacionDidactica PlaneacionDidactica { get; set; } = null!;

        public string Titulo { get; set; } = string.Empty;

        public ICollection<ChatParticipante> Participantes { get; set; } = new List<ChatParticipante>();

        public ICollection<ChatMensaje> Mensajes { get; set; } = new List<ChatMensaje>();
    }
}
