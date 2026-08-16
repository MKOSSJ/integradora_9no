namespace Plandi.Dto;

public class ChatMensajeDto
{
    public long Id { get; set; }

    public long ChatId { get; set; }

    public long UsuarioId { get; set; }

    public string Autor { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public string TipoMensaje { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public DateTime? EditadoAt { get; set; }
}
