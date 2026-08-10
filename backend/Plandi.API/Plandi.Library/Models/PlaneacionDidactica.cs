using Plandi.Dto.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class PlaneacionDidactica : BaseEntity
    {
        public long PeriodoId { get; set; }
        public Periodo Periodo { get; set; } = null!;

        public long AsignaturaId { get; set; }

        public Academia? Academia { get; set; }

        public long? RevisorId { get; set; }
        public Usuario? Revisor { get; set; }


        public EstadoPlaneacion Estado { get; set; } = EstadoPlaneacion.Borrador;

        public long? UltimaModificacionPorId { get; set; }
        public Usuario? UltimaModificacionPor { get; set; }

        public DateTime? FechaUltimaModificacion { get; set; }

        public long? CreatedBy { get; set; }

        // Relationships
        public PlaneacionCaratula? Caratula { get; set; }



        public ICollection<PlaneacionUnidad> Unidades { get; set; } = new List<PlaneacionUnidad>();

        public ICollection<PlaneacionReferencia> Referencias { get; set; } = new List<PlaneacionReferencia>();

        public ICollection<PlaneacionObservacion> Observaciones { get; set; } = new List<PlaneacionObservacion>();

        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
    }
}
