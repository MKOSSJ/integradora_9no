namespace Plandi.Dto;

public class CrearPlaneacionObservacionRequestDto
{
    public long? UnidadId { get; set; }

    public string Comentario { get; set; } = string.Empty;
}

// public class PlaneacionObservacionDto
// {
//     public long Id { get; set; }

//     public long PlaneacionDidacticaId { get; set; }

//     public long? UnidadId { get; set; }

//     public long RevisorId { get; set; }

//     public string Comentario { get; set; } = string.Empty;

//     public string Estado { get; set; } = string.Empty;

//     public string Autor { get; set; } = string.Empty;

//     public string Fecha { get; set; } = string.Empty;
// }
