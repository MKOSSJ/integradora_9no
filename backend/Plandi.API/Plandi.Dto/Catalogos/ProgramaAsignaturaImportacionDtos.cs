using System.Text.Json.Serialization;

namespace Plandi.Dto.Catalogos;

public class ProgramaAsignaturaImportacionResultadoDto
{
    public string Archivo { get; set; } = string.Empty;
    public Guid? ProgramaAsignaturaPublicId { get; set; }
    public string? Asignatura { get; set; }
    public string? Clave { get; set; }
    public int UnidadesExtraidas { get; set; }
    public bool DatosGuardados { get; set; }
    public List<string> Errores { get; set; } = [];
}

public class GeneracionPlaneacionesResultadoDto
{
    public int TotalProgramas { get; set; }
    public int PlaneacionesCreadas { get; set; }
    public int YaExistentes { get; set; }
    public int Omitidas { get; set; }
    public List<GeneracionPlaneacionDetalleDto> Planeaciones { get; set; } = [];
}

public class GeneracionPlaneacionDetalleDto
{
    public Guid ProgramaAsignaturaPublicId { get; set; }
    public string Asignatura { get; set; } = string.Empty;
    public Guid? PlaneacionPublicId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
}

public class ProgramaAsignaturaExtraidoDto
{
    public string ProgramaEducativo { get; set; } = string.Empty;
    public string NombreAsignatura { get; set; } = string.Empty;
    public string ClaveAsignatura { get; set; } = string.Empty;
    public string? Proposito { get; set; }
    public string? Competencia { get; set; }
    public string? TipoCompetencia { get; set; }
    public int? Cuatrimestre { get; set; }
    public decimal? Creditos { get; set; }
    public string? Modalidad { get; set; }
    public int? HorasSaber { get; set; }
    public int? HorasSaberHacer { get; set; }
    public int? HorasTotales { get; set; }
    public int? HorasSemana { get; set; }
    public string? Funciones { get; set; }
    public string? Capacidades { get; set; }
    public string? CriteriosDesempeno { get; set; }
    public string? PerfilIdoneoDocente { get; set; }
    public List<UnidadProgramaExtraidaDto> Unidades { get; set; } = [];
    public List<ReferenciaBibliograficaExtraidaDto> ReferenciasBibliograficas { get; set; } = [];
    public List<ReferenciaDigitalExtraidaDto> ReferenciasDigitales { get; set; } = [];
}

public class UnidadProgramaExtraidaDto
{
    public int Numero { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Proposito { get; set; }
    public TiempoAsignadoUnidadExtraidoDto TiempoAsignado { get; set; } = new();
    [JsonIgnore] public int? HorasSaber => TiempoAsignado.HorasSaber;
    [JsonIgnore] public int? HorasSaberHacer => TiempoAsignado.HorasSaberHacer;
    [JsonIgnore] public int? HorasTotales => TiempoAsignado.HorasTotales;
    public List<TemaProgramaExtraidoDto> Temas { get; set; } = [];
    public ProcesoEvaluacionUnidadExtraidoDto ProcesoEvaluacion { get; set; } = new();
}

public class TemaProgramaExtraidoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Saber { get; set; }
    public string? SaberHacer { get; set; }
    public string? SerConvivir { get; set; }
}

public class TiempoAsignadoUnidadExtraidoDto
{
    public int? HorasSaber { get; set; }
    public int? HorasSaberHacer { get; set; }
    public int? HorasTotales { get; set; }
}

public class ProcesoEvaluacionUnidadExtraidoDto
{
    public string? ResultadoAprendizaje { get; set; }
    public string? EvidenciaAprendizaje { get; set; }
    public string? InstrumentosEvaluacion { get; set; }
}

public class ReferenciaBibliograficaExtraidaDto
{
    public string? Autor { get; set; }
    public string? Anio { get; set; }
    public string? Titulo { get; set; }
    public string? LugarPublicacion { get; set; }
    public string? Editorial { get; set; }
    public string? Isbn { get; set; }
}

public class ReferenciaDigitalExtraidaDto
{
    public string? Autor { get; set; }
    public string? FechaRecuperacion { get; set; }
    public string? Titulo { get; set; }
    public string? Vinculo { get; set; }
}
