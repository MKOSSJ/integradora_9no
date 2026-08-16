using Plandi.Dto.Enums;

namespace Plandi.Dto;

public class PlaneacionEstadoDto
{
    public long Id { get; set; }

    public EstadoPlaneacion Estado { get; set; }

    public string FechaUltimaModificacion { get; set; } = string.Empty;

    public long? UsuarioUltimaModificacion { get; set; }
}
