using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionObservacion : BaseEntity
    {
        public long PlaneacionDidacticaId { get; set; }
        public PlaneacionDidactica PlaneacionDidactica { get; set; } = null!;

        public long? PlaneacionUnidadId { get; set; }
        public PlaneacionUnidad? PlaneacionUnidad { get; set; }

        public long RevisorId { get; set; }
        public Usuario Revisor { get; set; } = null!;

        public string Comentario { get; set; } = string.Empty;

        public string Estado { get; set; } = "ABIERTA";
    }
}
