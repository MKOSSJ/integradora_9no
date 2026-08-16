namespace Plandi.Dto;

public class ChatMensajesPaginadosDto
{
    public int Pagina { get; set; }

    public int TamanioPagina { get; set; }

    public int Total { get; set; }

    public int TotalPaginas { get; set; }

    public bool HasNext { get; set; }

    public bool HasPrevious { get; set; }

    public List<ChatMensajeDto> Mensajes { get; set; } = new();
}
