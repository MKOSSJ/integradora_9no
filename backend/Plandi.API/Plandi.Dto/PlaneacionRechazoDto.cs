using Plandi.Dto.Enums;

namespace Plandi.Dto;

public class PlaneacionRechazarRequestDto
{
    public string Motivo { get; set; } = string.Empty;
}

public class PlaneacionRechazoDto
{
    public long Id { get; set; }

    public EstadoPlaneacion Estado { get; set; }

    public string FechaUltimaModificacion { get; set; } = string.Empty;

    public long? UsuarioUltimaModificacion { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public long ObservacionId { get; set; }
}
