using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces;

public sealed record ArchivoContenido(byte[] Bytes, string MimeType, string NombreDescarga);

public interface IPlaneacionTemplateService
{
    Task<PlantillaPlaneacionDto> SubirAsync(Stream archivo, string nombreArchivo, string? mimeType, long usuarioId, CancellationToken cancellationToken = default);
    Task<PlantillaPlaneacionDto?> ObtenerActivaAsync(CancellationToken cancellationToken = default);
    Task<ArchivoContenido> ObtenerArchivoAsync(Guid plantillaPublicId, CancellationToken cancellationToken = default);
    Task<ArchivoContenido> ObtenerArchivoActivoAsync(CancellationToken cancellationToken = default);
}

public interface IPlaneacionPdfService
{
    Task<ArchivoContenido> GenerarPdfAsync(Guid planeacionPublicId, long usuarioId, CancellationToken cancellationToken = default);
}

public interface IPlaneacionDocumentosService
{
    Task<PlaneacionDetalleConArchivosDto> ObtenerDetalleAsync(Guid planeacionPublicId, long usuarioId, CancellationToken cancellationToken = default);
    Task<ArchivoContenido> ObtenerProgramaAsync(Guid programaPublicId, long usuarioId, CancellationToken cancellationToken = default);
}
