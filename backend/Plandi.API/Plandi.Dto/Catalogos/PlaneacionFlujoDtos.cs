using Plandi.Dto.Enums;

namespace Plandi.Dto.Catalogos;

public sealed class PlaneacionResumenDto
{
    public Guid PublicId { get; set; }
    public string Asignatura { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public string Grupos { get; set; } = string.Empty;
    public string Docentes { get; set; } = string.Empty;
    public EstadoPlaneacion Estado { get; set; }
    public Guid? RevisorPublicId { get; set; }
    public string? Revisor { get; set; }
    public DateTime? UltimaModificacion { get; set; }
}

public sealed class PlaneacionEdicionDto
{
    public Guid PublicId { get; set; }
    public EstadoPlaneacion Estado { get; set; }
    public CaratulaPlaneacionEdicionDto Caratula { get; set; } = new();
    public List<UnidadPlaneacionEdicionDto> Unidades { get; set; } = [];
    public List<ReferenciaPlaneacionEdicionDto> Referencias { get; set; } = [];
}

public sealed class CaratulaPlaneacionEdicionDto
{
    public string? ProgramaEducativo { get; set; }
    public int? Cuatrimestre { get; set; }
    public string? NombreAsignatura { get; set; }
    public string? Docentes { get; set; }
    public string? PeriodoEscolar { get; set; }
    public string? Grupos { get; set; }
    public string? PropositoAsignatura { get; set; }
    public string? CompetenciaAsignatura { get; set; }
    public string? TipoCompetencia { get; set; }
    public decimal? Creditos { get; set; }
    public string? Modalidad { get; set; }
    public int? HorasSaber { get; set; }
    public int? HorasSaberHacer { get; set; }
    public int? HorasTotales { get; set; }
    public int? HorasSemana { get; set; }
}

public sealed class UnidadPlaneacionEdicionDto
{
    public long? Id { get; set; }
    public int? NumeroUnidad { get; set; }
    public string NombreUnidad { get; set; } = string.Empty;
    public string? PropositoEsperado { get; set; }
    public int? HorasSaber { get; set; }
    public int? HorasSaberHacer { get; set; }
    public int? HorasTotales { get; set; }
    public decimal? PorcentajeUnidad { get; set; }
    public int Orden { get; set; }
    public List<TemaPlaneacionEdicionDto> Temas { get; set; } = [];
    public List<EvaluacionPlaneacionEdicionDto> Evaluaciones { get; set; } = [];
    public List<SecuenciaPlaneacionEdicionDto> Secuencias { get; set; } = [];
}

public sealed class TemaPlaneacionEdicionDto
{
    public long? Id { get; set; }
    public string Tema { get; set; } = string.Empty;
    public string? SaberConceptual { get; set; }
    public string? SaberHacer { get; set; }
    public string? SaberSer { get; set; }
    public int Orden { get; set; }
}

public sealed class EvaluacionPlaneacionEdicionDto
{
    public long? Id { get; set; }
    public int? PeriodoSemanas { get; set; }
    public string? ResultadoAprendizaje { get; set; }
    public string? EvidenciaAprendizaje { get; set; }
    public FaseSecuencia Fase { get; set; }
    public TipoEvaluacion? TipoEvaluacion { get; set; }
    public AgenteEvaluador AgenteEvaluador { get; set; }
    public decimal? Ponderacion { get; set; }
    public string? InstrumentoEvaluacion { get; set; }
    public int Orden { get; set; }
}

public sealed class SecuenciaPlaneacionEdicionDto
{
    public long? Id { get; set; }
    public FaseSecuencia Fase { get; set; }
    public int Estrategia { get; set; }
    public string? ActividadDocente { get; set; }
    public string? ActividadEstudiante { get; set; }
    public string? EvidenciaAprendizaje { get; set; }
    public string? MediosMateriales { get; set; }
    public int Orden { get; set; }
}

public sealed class ReferenciaPlaneacionEdicionDto
{
    public long? Id { get; set; }
    public string ReferenciaAPA { get; set; } = string.Empty;
    public int Orden { get; set; }
}

public sealed class AsignarRevisorPlaneacionDto { public Guid RevisorPublicId { get; set; } }
public sealed class CambioEstadoPlaneacionDto { public EstadoPlaneacion Estado { get; set; } }
