using System;
using System.Collections.Generic;
using System.Text;
using Plandi.Dto.Enums;

namespace Plandi.Library.Models
{
    public class Periodo : BaseEntity
    {
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }

        public long CicloEscolarId { get; set; }
        public CicloEscolar CicloEscolar { get; set; } = null!;

        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public EstadoPeriodo Estado { get; set; } = EstadoPeriodo.Activo;

        public DateTime? FechaCierre { get; set; }

        public ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();
    }
}
