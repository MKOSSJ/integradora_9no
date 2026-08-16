using Plandi.Dto.Enums;
using Plandi.Library.Models;

namespace Plandi.Services.Interfaces;

public interface IRelojAcademico
{
    DateTime AhoraLocal { get; }
    DateTime AhoraUtc { get; }
}

public interface IPeriodoLifecycleService
{
    EstadoPeriodo ObtenerEstadoEfectivo(Periodo periodo);
    bool PermiteModificaciones(Periodo periodo);
    Task ExigirEditableAsync(long periodoId, CancellationToken cancellationToken = default);
    Task<int> ActualizarEstadosAsync(CancellationToken cancellationToken = default);
    Task CerrarAsync(long periodoId, long actorId, CancellationToken cancellationToken = default);
}
