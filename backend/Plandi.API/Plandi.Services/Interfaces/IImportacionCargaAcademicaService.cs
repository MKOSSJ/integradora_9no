using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces;

public interface IImportacionCargaAcademicaService
{
    Task<ImportacionCargaAcademicaResultadoDto> Importar(
        Stream archivo,
        string nombreArchivo,
        Guid periodoPublicId,
        CancellationToken cancellationToken = default);
}
