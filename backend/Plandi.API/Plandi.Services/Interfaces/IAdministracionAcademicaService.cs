using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;

namespace Plandi.Services.Interfaces;

public interface IAdministracionAcademicaService
{
    Task<PagedResult<AdminUsuarioDto>> UsuariosAsync(AdminConsultaDto filtro, CancellationToken ct = default);
    Task<AdminUsuarioDto> UsuarioAsync(Guid publicId, CancellationToken ct = default);
    Task<PagedResult<AdminGrupoDto>> GruposAsync(AdminConsultaDto filtro, CancellationToken ct = default);
    Task<AdminGrupoDto> GrupoAsync(Guid publicId, CancellationToken ct = default);
    Task<PagedResult<AdminAsignaturaDto>> AsignaturasAsync(AdminConsultaDto filtro, CancellationToken ct = default);
    Task<AdminAsignaturaDto> AsignaturaAsync(Guid publicId, CancellationToken ct = default);
    Task<PagedResult<AdminCicloDto>> CiclosAsync(AdminConsultaDto filtro, CancellationToken ct = default);
    Task<AdminCicloDto> CicloAsync(Guid publicId, CancellationToken ct = default);
    Task<PagedResult<AdminPeriodoDto>> PeriodosAsync(AdminConsultaDto filtro, CancellationToken ct = default);
    Task<AdminPeriodoDto> PeriodoAsync(Guid publicId, CancellationToken ct = default);
    Task<PagedResult<AdminCargaResumenDto>> CargasAsync(AdminConsultaDto filtro, CancellationToken ct = default);
    Task<AdminCargaResumenDto> CargaAsync(Guid publicId, CancellationToken ct = default);
}
