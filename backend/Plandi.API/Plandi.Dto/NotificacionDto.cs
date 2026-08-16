namespace Plandi.Dto;

public class NotificacionDto
{
    public long Id { get; set; }

    public long UsuarioId { get; set; }

    public long? PlaneacionDidacticaId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public bool Leida { get; set; }

    public DateTime? FechaLectura { get; set; }

    public DateTime CreatedAt { get; set; }
}
