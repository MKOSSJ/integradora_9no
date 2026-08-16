using System.ComponentModel.DataAnnotations;
using Plandi.Dto.Enums;

namespace Plandi.Dto.Catalogos;

public sealed class RepositorioPlaneacionesFiltroDto
{
    public Guid? PeriodoPublicId { get; set; }
    public Guid? AsignaturaPublicId { get; set; }
    public Guid? DocentePublicId { get; set; }
    public Guid? CicloPublicId { get; set; }
    public Guid? GrupoPublicId { get; set; }
    public Guid? CarreraPublicId { get; set; }
    public Guid? AcademiaPublicId { get; set; }
    public EstadoPlaneacion? EstadoPlaneacion { get; set; }
    [MaxLength(200)] public string? Search { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
}

public sealed class RepositorioArchivoDto
{
    public string Tipo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long? TamanoBytes { get; set; }
    public bool Disponible { get; set; }
    public string UrlDescarga { get; set; } = string.Empty;
}

public sealed class RepositorioPlaneacionDto
{
    public Guid PublicId { get; set; }
    public AdminEntidadResumenDto Asignatura { get; set; } = new();
    public AdminPeriodoResumenDto Periodo { get; set; } = new();
    public AdminEntidadResumenDto Ciclo { get; set; } = new();
    public IReadOnlyList<AdminEntidadResumenDto> Docentes { get; set; } = [];
    public IReadOnlyList<AdminEntidadResumenDto> Grupos { get; set; } = [];
    public IReadOnlyList<AdminEntidadResumenDto> Carreras { get; set; } = [];
    public AdminEntidadResumenDto? Academia { get; set; }
    public EstadoPlaneacion EstadoPlaneacion { get; set; }
    public DateTime Fecha { get; set; }
    public bool SoloLectura { get; set; } = true;
    public IReadOnlyList<RepositorioArchivoDto> Archivos { get; set; } = [];
}
