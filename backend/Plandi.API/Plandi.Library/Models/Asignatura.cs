using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public class Asignatura : BaseEntity
    {
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }

        public long? AcademiaId { get; set; }
        public Academia? Academia { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Clave { get; set; } = string.Empty;

        public int Cuatrimestre { get; set; }

        public int HorasTotales { get; set; }

        public int HorasSemana { get; set; }

        public decimal Creditos { get; set; }

        public ICollection<CargaAcademica> CargasAcademicas { get; set; } = new List<CargaAcademica>();

        public ICollection<ProgramaAsignatura> ProgramasAsignatura { get; set; } = new List<ProgramaAsignatura>();

        public ICollection<PlaneacionDidactica> PlaneacionesDidacticas { get; set; } = new List<PlaneacionDidactica>();
    }
}
