using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces;

public interface IProgramaAsignaturaImportService
{
    Task<string> ExtraerTextoAsync(Stream archivo, string nombreArchivo, CancellationToken cancellationToken = default);

    Task<ProgramaAsignaturaImportacionResultadoDto> ImportarAsync(Stream archivo, string nombreArchivo,
        long tamanoBytes, string? mimeType, long subidoPorId, string directorioStorage,
        CancellationToken cancellationToken = default);
}
