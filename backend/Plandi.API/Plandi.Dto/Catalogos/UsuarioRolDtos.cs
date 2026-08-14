namespace Plandi.Dto.Catalogos;

public sealed class RolUsuarioDto
{
    public Guid PublicId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public sealed class UsuarioRolesDto
{
    public Guid UsuarioPublicId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public List<RolUsuarioDto> Roles { get; set; } = [];
}

public sealed class AsignarRolUsuarioDto
{
    public Guid RolPublicId { get; set; }
}
