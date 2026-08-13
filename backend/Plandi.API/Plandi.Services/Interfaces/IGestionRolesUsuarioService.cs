using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces;

public interface IGestionRolesUsuarioService
{
    Task<IReadOnlyList<RolUsuarioDto>> ObtenerCatalogoAsync(long usuarioAutorizadoId, CancellationToken cancellationToken = default);
    Task<UsuarioRolesDto> ObtenerRolesAsync(Guid usuarioPublicId, long usuarioAutorizadoId, CancellationToken cancellationToken = default);
    Task<UsuarioRolesDto> AsignarAsync(Guid usuarioPublicId, Guid rolPublicId, long usuarioAutorizadoId, CancellationToken cancellationToken = default);
    Task<UsuarioRolesDto> QuitarAsync(Guid usuarioPublicId, Guid rolPublicId, long usuarioAutorizadoId, CancellationToken cancellationToken = default);
}
