namespace Plandi.Library.Models;

/// <summary>Metadata for an institutional planning template. The file itself is represented by Documento.</summary>
public sealed class PlaneacionTemplate : BaseEntity
{
    public long DocumentoId { get; set; }
    public Documento Documento { get; set; } = null!;
    public int Version { get; set; }
    public bool Activa { get; set; }
}
