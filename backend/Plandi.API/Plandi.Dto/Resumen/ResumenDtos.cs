namespace Plandi.Dto.Resumen;

public sealed class ResumenDashboardDto
{
    public int UsuariosRegistrados { get; init; }
    public int Academias { get; init; }
    public int GruposActivos { get; init; }
    public int Importaciones { get; init; }
    public decimal AvancePlaneaciones { get; init; }
}

public sealed class ResumenUsuariosDto
{
    public int Total { get; init; }
    public int Docentes { get; init; }
    public int Revisores { get; init; }
    public int Directores { get; init; }
}

public sealed class ResumenCarrerasDto
{
    public int Total { get; init; }
    public int Activas { get; init; }
    public int Inactivas { get; init; }
}

public sealed class ResumenAsignaturasDto
{
    public int Total { get; init; }
    public int Activas { get; init; }
    public int Inactivas { get; init; }
}

public sealed class ResumenCiclosEscolaresDto
{
    public int Total { get; init; }
    public int Activos { get; init; }
    public int Inactivos { get; init; }
}

public sealed class ResumenPeriodosDto
{
    public int Total { get; init; }
    public int Activos { get; init; }
    public int Inactivos { get; init; }
}

public sealed class ResumenGruposDto
{
    public int Total { get; init; }
    public int Activos { get; init; }
    public int Inactivos { get; init; }
}

public sealed class ResumenAsignacionAcademicaDto
{
    public int Total { get; init; }
    public int Activas { get; init; }
    public int Inactivas { get; init; }
}

public sealed class ResumenSeguimientoPlaneacionesDto
{
    public int Total { get; init; }
    public int Completadas { get; init; }
    public int EnRevision { get; init; }
    public int PorVencer { get; init; }
}

public sealed class ResumenDashboardDocenteDto
{
    public int Planeaciones { get; init; }
    public int Aprobadas { get; init; }
    public int Pendientes { get; init; }
}

public sealed class ResumenPlaneacionesDocenteDto
{
    public int Total { get; init; }
    public int Borrador { get; init; }
    public int Revision { get; init; }
    public int Aprobadas { get; init; }
}

public sealed class ResumenDashboardRevisorDto
{
    public int Planeaciones { get; init; }
    public int Validadas { get; init; }
    public int Correcciones { get; init; }
    public int PlaneacionesAValidar { get; init; }
}

public sealed class ResumenValidacionDto
{
    public int Pendientes { get; init; }
    public int Aprobadas { get; init; }
    public int Correcciones { get; init; }
}
