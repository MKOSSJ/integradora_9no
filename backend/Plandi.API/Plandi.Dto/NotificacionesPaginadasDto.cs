namespace Plandi.Dto;

public class NotificacionesPaginadasDto
{
    public int Pagina { get; set; }

    public int TamanioPagina { get; set; }

    public int Total { get; set; }

    public int TotalPaginas { get; set; }

    public bool HasNext { get; set; }

    public bool HasPrevious { get; set; }

    public List<NotificacionDto> Notificaciones { get; set; } = new();
}
