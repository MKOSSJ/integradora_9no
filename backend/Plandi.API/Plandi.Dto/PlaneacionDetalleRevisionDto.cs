using Plandi.Dto.Enums;

namespace Plandi.Dto;

public class PlaneacionDetalleRevisionDto
{
    public long Id { get; set; }

    public PlaneacionDetalleRevisionCaratulaDto Caratula { get; set; } = new();

    public List<PlaneacionDetalleRevisionObservacionDto> Observaciones { get; set; } = new();

    public List<PlaneacionDetalleRevisionUnidadDto> Unidades { get; set; } = new();
}

public class PlaneacionDetalleRevisionCaratulaDto
{
    public string Titulo { get; set; } = string.Empty;

    public string Asignatura { get; set; } = string.Empty;

    public string Periodo { get; set; } = string.Empty;

    public string Academia { get; set; } = string.Empty;

    public EstadoPlaneacion Estado { get; set; }

    public PlaneacionDetalleRevisionUltimaModificacionDto UltimaModificacion { get; set; } = new();
}

public class PlaneacionDetalleRevisionUltimaModificacionDto
{
    public string Fecha { get; set; } = string.Empty;

    public string Usuario { get; set; } = string.Empty;
}

public class PlaneacionDetalleRevisionUnidadDto
{
    public long Id { get; set; }

    public string Numero { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public List<PlaneacionDetalleRevisionObservacionDto> Observaciones { get; set; } = new();

    public List<PlaneacionDetalleRevisionActividadesPorSemanaDto> ActividadesPorSemana { get; set; } = new();
}

public class PlaneacionDetalleRevisionObservacionDto
{
    public long Id { get; set; }

    public string Comentario { get; set; } = string.Empty;

    public string Autor { get; set; } = string.Empty;

    public string Fecha { get; set; } = string.Empty;
}

public class PlaneacionDetalleRevisionActividadesPorSemanaDto
{
    public string Titulo { get; set; } = string.Empty;

    public int? Semana { get; set; }

    public List<PlaneacionDetalleRevisionActividadDto> Actividades { get; set; } = new();
}

public class PlaneacionDetalleRevisionActividadDto
{
    public long Id { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public int? Semana { get; set; }

    public int? Horas { get; set; }

    public string Estrategia { get; set; } = string.Empty;

    public string Evidencia { get; set; } = string.Empty;

    public string Instrumento { get; set; } = string.Empty;

    public decimal? PorcentajeEvaluacion { get; set; }
}
