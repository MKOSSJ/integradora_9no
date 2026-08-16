using System.ComponentModel.DataAnnotations;
using Plandi.Dto.Enums;

namespace Plandi.Dto.Catalogos;

public sealed class AdminConsultaDto
{
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    [MaxLength(200)] public string? Search { get; set; }
}

public class AdminEntidadResumenDto
{
    public Guid PublicId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Clave { get; set; }
}

public sealed class AdminPeriodoResumenDto : AdminEntidadResumenDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public EstadoPeriodo Estado { get; set; }
    public bool PermiteModificaciones { get; set; }
}

public sealed class AdminUsuarioDto
{
    public Guid PublicId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string? ApellidoMaterno { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public bool Activo { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public IReadOnlyList<AdminEntidadResumenDto> Academias { get; set; } = [];
    public int TotalCargasAcademicas { get; set; }
}

public sealed class AdminCargaResumenDto
{
    public Guid PublicId { get; set; }
    public AdminEntidadResumenDto Docente { get; set; } = new();
    public AdminEntidadResumenDto Asignatura { get; set; } = new();
    public AdminEntidadResumenDto Grupo { get; set; } = new();
    public AdminPeriodoResumenDto Periodo { get; set; } = new();
    public AdminEntidadResumenDto Ciclo { get; set; } = new();
    public AdminEntidadResumenDto Programa { get; set; } = new();
    public AdminEntidadResumenDto? Academia { get; set; }
    public AdminEntidadResumenDto? Revisor { get; set; }
    public Guid? PlaneacionPublicId { get; set; }
    public EstadoPlaneacion? EstadoPlaneacion { get; set; }
    public int HorasTotales { get; set; }
    public int HorasSemana { get; set; }
    public bool Activa { get; set; }
    public bool PermiteModificaciones { get; set; }
}

public sealed class AdminGrupoDto
{
    public Guid PublicId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cuatrimestre { get; set; }
    public AdminEntidadResumenDto Carrera { get; set; } = new();
    public AdminPeriodoResumenDto Periodo { get; set; } = new();
    public AdminEntidadResumenDto Ciclo { get; set; } = new();
    public bool Activo { get; set; }
    public bool PermiteModificaciones { get; set; }
    public IReadOnlyList<AdminCargaResumenDto> Asignaciones { get; set; } = [];
}

public sealed class AdminAsignaturaDto
{
    public Guid PublicId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public int Cuatrimestre { get; set; }
    public int HorasTotales { get; set; }
    public int HorasSemana { get; set; }
    public decimal Creditos { get; set; }
    public bool Activo { get; set; }
    public AdminEntidadResumenDto? Academia { get; set; }
    public IReadOnlyList<AdminEntidadResumenDto> ProgramasAsignatura { get; set; } = [];
    public IReadOnlyList<AdminCargaResumenDto> Imparticiones { get; set; } = [];
}

public sealed class AdminPeriodoDto
{
    public Guid PublicId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public EstadoPeriodo Estado { get; set; }
    public EstadoPeriodo EstadoEfectivo { get; set; }
    public DateTime? FechaCierre { get; set; }
    public bool PermiteModificaciones { get; set; }
    public AdminEntidadResumenDto Ciclo { get; set; } = new();
    public int TotalGrupos { get; set; }
    public int TotalCargas { get; set; }
    public int TotalPlaneaciones { get; set; }
}

public sealed class AdminCicloDto
{
    public Guid PublicId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; }
    public IReadOnlyList<AdminPeriodoDto> Periodos { get; set; } = [];
}

public sealed class ActualizarGrupoCargaAcademicaDto
{
    [Required] public Guid GrupoPublicId { get; set; }
}
