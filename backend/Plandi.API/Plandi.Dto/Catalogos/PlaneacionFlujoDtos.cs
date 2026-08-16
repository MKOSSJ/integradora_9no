using Plandi.Dto.Enums;
using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Catalogos;

public sealed class PlaneacionResumenDto
{
    public Guid PublicId { get; set; }
    public string Asignatura { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public string Grupos { get; set; } = string.Empty;
    public string Docentes { get; set; } = string.Empty;
    [EnumDataType(typeof(EstadoPlaneacion))]
    public EstadoPlaneacion Estado { get; set; }
    public Guid? RevisorPublicId { get; set; }
    public string? Revisor { get; set; }
    public DateTime? UltimaModificacion { get; set; }
}

public sealed class PlaneacionEdicionDto
{
    public Guid PublicId { get; set; }
    public EstadoPlaneacion Estado { get; set; }
    [Required]
    public CaratulaPlaneacionEdicionDto Caratula { get; set; } = new();
    public List<UnidadPlaneacionEdicionDto> Unidades { get; set; } = [];
    public List<ReferenciaPlaneacionEdicionDto> Referencias { get; set; } = [];
}

public sealed class CaratulaPlaneacionEdicionDto
{
    [MaxLength(200)]
    public string? ProgramaEducativo { get; set; }
    [Range(1, int.MaxValue)]
    public int? Cuatrimestre { get; set; }
    [MaxLength(200)]
    public string? NombreAsignatura { get; set; }
    public string? Docentes { get; set; }
    [MaxLength(100)]
    public string? PeriodoEscolar { get; set; }
    [MaxLength(500)]
    public string? Grupos { get; set; }
    public string? PropositoAsignatura { get; set; }
    public string? CompetenciaAsignatura { get; set; }
    [MaxLength(100)]
    public string? TipoCompetencia { get; set; }
    [Range(typeof(decimal), "0", "999.99")]
    public decimal? Creditos { get; set; }
    [MaxLength(100)]
    public string? Modalidad { get; set; }
    [Range(0, int.MaxValue)]
    public int? HorasSaber { get; set; }
    [Range(0, int.MaxValue)]
    public int? HorasSaberHacer { get; set; }
    [Range(0, int.MaxValue)]
    public int? HorasTotales { get; set; }
    [Range(0, int.MaxValue)]
    public int? HorasSemana { get; set; }
}

public sealed class UnidadPlaneacionEdicionDto
{
    public Guid? PublicId { get; set; }
    public int? NumeroUnidad { get; set; }
    [Required]
    [MaxLength(250)]
    public string NombreUnidad { get; set; } = string.Empty;
    public string? PropositoEsperado { get; set; }
    [Range(0, int.MaxValue)]
    public int? HorasSaber { get; set; }
    [Range(0, int.MaxValue)]
    public int? HorasSaberHacer { get; set; }
    [Range(0, int.MaxValue)]
    public int? HorasTotales { get; set; }
    [Range(typeof(decimal), "0", "100")]
    public decimal? PorcentajeUnidad { get; set; }
    [Range(0, int.MaxValue)]
    public int Orden { get; set; }
    public List<TemaPlaneacionEdicionDto> Temas { get; set; } = [];
    public List<EvaluacionPlaneacionEdicionDto> Evaluaciones { get; set; } = [];
    public List<SecuenciaPlaneacionEdicionDto> Secuencias { get; set; } = [];
}

public sealed class TemaPlaneacionEdicionDto
{
    public Guid? PublicId { get; set; }
    [Required]
    [MaxLength(250)]
    public string Tema { get; set; } = string.Empty;
    public string? SaberConceptual { get; set; }
    public string? SaberHacer { get; set; }
    public string? SaberSer { get; set; }
    [Range(0, int.MaxValue)]
    public int Orden { get; set; }
}

public sealed class EvaluacionPlaneacionEdicionDto
{
    public Guid? PublicId { get; set; }
    public int? PeriodoSemanas { get; set; }
    public string? ResultadoAprendizaje { get; set; }
    public string? EvidenciaAprendizaje { get; set; }
    [EnumDataType(typeof(FaseSecuencia))]
    public FaseSecuencia Fase { get; set; }
    [EnumDataType(typeof(TipoEvaluacion))]
    public TipoEvaluacion? TipoEvaluacion { get; set; }
    [EnumDataType(typeof(AgenteEvaluador))]
    public AgenteEvaluador AgenteEvaluador { get; set; }
    [Range(typeof(decimal), "0", "100")]
    public decimal? Ponderacion { get; set; }
    public string? InstrumentoEvaluacion { get; set; }
    [Range(0, int.MaxValue)]
    public int Orden { get; set; }
}

public sealed class SecuenciaPlaneacionEdicionDto
{
    public Guid? PublicId { get; set; }
    [EnumDataType(typeof(FaseSecuencia))]
    public FaseSecuencia Fase { get; set; }
    [Range(1, int.MaxValue)]
    public int Estrategia { get; set; }
    public string? ActividadDocente { get; set; }
    public string? ActividadEstudiante { get; set; }
    public string? EvidenciaAprendizaje { get; set; }
    public string? MediosMateriales { get; set; }
    [Range(0, int.MaxValue)]
    public int Orden { get; set; }
}

public sealed class ReferenciaPlaneacionEdicionDto
{
    public Guid? PublicId { get; set; }
    [Required]
    public string ReferenciaAPA { get; set; } = string.Empty;
    [Range(0, int.MaxValue)]
    public int Orden { get; set; }
}

public sealed class AsignarRevisorPlaneacionDto { [Required] public Guid RevisorPublicId { get; set; } }
public sealed class CambioEstadoPlaneacionDto { [EnumDataType(typeof(EstadoPlaneacion))] public EstadoPlaneacion Estado { get; set; } }
