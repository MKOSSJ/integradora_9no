using System.ComponentModel.DataAnnotations;
using Plandi.Dto.Enums;

namespace Plandi.Dto.Catalogos;

public sealed class CrearComentarioCorreccionDto
{
    [Required]
    public string Mensaje { get; set; } = string.Empty;
}

public sealed class ComentarioCorreccionDto
{
    public Guid PublicId { get; set; }
    public Guid UsuarioPublicId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string RolEnChat { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public sealed class ComentariosCorreccionDto
{
    public EstadoPlaneacion EstadoPlaneacion { get; set; }
    public bool OcultosPorAprobacion { get; set; }
    public IReadOnlyList<ComentarioCorreccionDto> Comentarios { get; set; } = [];
}
