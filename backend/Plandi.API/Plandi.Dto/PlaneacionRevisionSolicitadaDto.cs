using Plandi.Dto.Enums;

namespace Plandi.Dto;

public class PlaneacionRevisionSolicitadaDto
{
    public long Id { get; set; }

    public EstadoPlaneacion Estado { get; set; }

    public long ChatId { get; set; }

    public int Participantes { get; set; }

    public string FechaUltimaModificacion { get; set; } = string.Empty;

    public long? UsuarioUltimaModificacion { get; set; }
}
