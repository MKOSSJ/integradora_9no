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
       
        public String? Seccion { get; set; }

        public long? PlaneacionTemaId { get; set; }

        public long? PlaneacionEvaluacionId { get; set; }

        public long? PlaneacionSecuenciaId { get; set; }

        public DateTime FechaRevision { get; set; }

        public DateTime? FechaAtendida { get; set; }
    }
}
