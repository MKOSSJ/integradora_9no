using System;

namespace Plandi.Library.Models
{
    public class Notificacion : BaseEntity
    {
        public long UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public long? PlaneacionDidacticaId { get; set; }
        public PlaneacionDidactica? PlaneacionDidactica { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public bool Leida { get; set; }

        public DateTime? FechaLectura { get; set; }
    }
}
