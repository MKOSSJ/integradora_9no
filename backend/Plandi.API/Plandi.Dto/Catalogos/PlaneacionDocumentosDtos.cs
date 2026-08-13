namespace Plandi.Dto.Catalogos;

public sealed class ArchivoRelacionadoDto
{
    public bool Disponible { get; set; }
    public string? Nombre { get; set; }
    public string? MimeType { get; set; }
    public string? UrlVisualizacion { get; set; }
    public string? UrlDescarga { get; set; }
}

public sealed class PlaneacionArchivosDto
{
    public ArchivoRelacionadoDto ProgramaAsignatura { get; set; } = new();
    public ArchivoRelacionadoDto PlaneacionDidactica { get; set; } = new();
}

public sealed class PlaneacionDetalleConArchivosDto
{
    public PlaneacionEdicionDto Planeacion { get; set; } = new();
    public PlaneacionArchivosDto Archivos { get; set; } = new();
}

public sealed class PlantillaPlaneacionDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Version { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaCarga { get; set; }
}
