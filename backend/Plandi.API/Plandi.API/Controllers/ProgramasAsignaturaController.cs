using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.API.Models;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Services;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers;

[ApiController]
[Route("api/programas-asignatura")]
public class ProgramasAsignaturaController(IProgramaAsignaturaImportService importacionService, IWebHostEnvironment environment, IAutorizacionService autorizacion, IPlaneacionDocumentosService documentos) : ControllerBase
{
    [Authorize]
    [HttpGet("{publicId:guid}/archivo")]
    public async Task<IActionResult> VisualizarArchivo(Guid publicId, CancellationToken cancellationToken)
    {
        var file = await documentos.ObtenerProgramaAsync(publicId, autorizacion.ObtenerUsuarioId(User), cancellationToken);
        return File(file.Bytes, file.MimeType);
    }

    [Authorize]
    [HttpGet("{publicId:guid}/archivo/descarga")]
    public async Task<IActionResult> DescargarArchivo(Guid publicId, CancellationToken cancellationToken)
    {
        var file = await documentos.ObtenerProgramaAsync(publicId, autorizacion.ObtenerUsuarioId(User), cancellationToken);
        return File(file.Bytes, file.MimeType, file.NombreDescarga);
    }
    [HttpPost("extraer-texto")]
    [Authorize(Roles = "Director")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ExtraerTexto([FromForm] ImportarProgramasAsignaturaForm file, CancellationToken cancellationToken)
    {
        var s = file.Files.FirstOrDefault();
        if (s is null || s.Length == 0) return BadRequest(ApiResponse<string>.Fail("Debe adjuntar un PDF."));
        if (s.Length > ProgramaAsignaturaImportService.MaxPdfBytes) return BadRequest(ApiResponse<string>.Fail("El PDF no puede exceder 25 MB."));

        try
        {
            await using var stream = s.OpenReadStream();
            var texto = await importacionService.ExtraerTextoAsync(stream, s.FileName, cancellationToken);
            var nombreTxt = $"{Path.GetFileNameWithoutExtension(s.FileName)}.txt";
            return File(System.Text.Encoding.UTF8.GetBytes(texto), "text/plain; charset=utf-8", nombreTxt);
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPost("importar")]
    [Authorize(Roles = "Director")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Importar([FromForm] ImportarProgramasAsignaturaForm request, CancellationToken cancellationToken)
    {
        if (request.Files.Count == 0) return BadRequest(ApiResponse<List<ProgramaAsignaturaImportacionResultadoDto>>.Fail("Debe adjuntar al menos un PDF."));
        if (request.Files.Any(file => file.Length == 0 || file.Length > ProgramaAsignaturaImportService.MaxPdfBytes))
            return BadRequest(ApiResponse<List<ProgramaAsignaturaImportacionResultadoDto>>.Fail("Cada PDF debe tener contenido y no exceder 25 MB."));
        var resultados = new List<ProgramaAsignaturaImportacionResultadoDto>();
        var directorio = Path.Combine(environment.ContentRootPath, "documentos", "programas-asignatura");
        foreach (var file in request.Files)
        {
            try
            {
                await using var stream = file.OpenReadStream();
                resultados.Add(await importacionService.ImportarAsync(stream, file.FileName, file.Length, file.ContentType, autorizacion.ObtenerUsuarioId(User), directorio, cancellationToken));
            }
            catch (AppException ex)
            {
                resultados.Add(new ProgramaAsignaturaImportacionResultadoDto { Archivo = file.FileName, Errores = [ex.Message] });
            }
        }
        return Ok(ApiResponse<List<ProgramaAsignaturaImportacionResultadoDto>>.Ok(resultados, "Importación de programas finalizada."));
    }
}
