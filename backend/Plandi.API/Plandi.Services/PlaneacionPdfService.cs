using System.Diagnostics;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using Plandi.Dto.Common;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services;

public sealed class PlaneacionPdfService(IPlaneacionDocumentosService documentos, IPlaneacionTemplateService templates, IConfiguration configuration) : IPlaneacionPdfService
{
    public async Task<ArchivoContenido> GenerarPdfAsync(Guid planeacionPublicId, long usuarioId, CancellationToken cancellationToken = default)
    {
        var detalle = await documentos.ObtenerDetalleAsync(planeacionPublicId, usuarioId, cancellationToken);
        var plantilla = await templates.ObtenerArchivoActivoAsync(cancellationToken);
        var work = Path.Combine(Path.GetTempPath(), "plandi-pdf", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(work);
        try
        {
            var docx = Path.Combine(work, "planeacion.docx");
            await File.WriteAllBytesAsync(docx, plantilla.Bytes, cancellationToken);
            CompletarDocumento(docx, detalle.Planeacion);
            var pdf = await ConvertirAPdfAsync(docx, work, cancellationToken);
            var fileName = PlaneacionTemplateService.NombreSeguro($"Planeacion_{detalle.Planeacion.Caratula.NombreAsignatura ?? detalle.Planeacion.PublicId.ToString()}.pdf", ".pdf");
            return new ArchivoContenido(await File.ReadAllBytesAsync(pdf, cancellationToken), "application/pdf", fileName);
        }
        finally { try { Directory.Delete(work, true); } catch { /* The OS will clear a locked temporary folder. */ } }
    }

    private static void CompletarDocumento(string path, Plandi.Dto.Catalogos.PlaneacionEdicionDto data)
    {
        using var document = WordprocessingDocument.Open(path, true);
        var body = document.MainDocumentPart?.Document.Body ?? throw new PdfGenerationException("La plantilla DOCX no contiene un documento principal.");
        var c = data.Caratula;
        var values = new Dictionary<string, string?>
        {
            ["PROGRAMA_EDUCATIVO"] = c.ProgramaEducativo, ["CUATRIMESTRE"] = c.Cuatrimestre?.ToString(), ["ASIGNATURA"] = c.NombreAsignatura,
            ["DOCENTES"] = c.Docentes, ["PERIODO"] = c.PeriodoEscolar, ["GRUPOS"] = c.Grupos, ["PROPOSITO"] = c.PropositoAsignatura,
            ["COMPETENCIA"] = c.CompetenciaAsignatura, ["CREDITOS"] = c.Creditos?.ToString(), ["MODALIDAD"] = c.Modalidad,
            ["HORAS_SABER"] = c.HorasSaber?.ToString(), ["HORAS_SABER_HACER"] = c.HorasSaberHacer?.ToString(), ["HORAS_TOTALES"] = c.HorasTotales?.ToString(), ["HORAS_SEMANA"] = c.HorasSemana?.ToString()
        };
        foreach (var text in body.Descendants<Text>())
        {
            foreach (var (key, value) in values) text.Text = text.Text.Replace($"{{{{{key}}}}}", value ?? string.Empty, StringComparison.Ordinal);
            text.Text = Regex.Replace(text.Text, @"(?<!\d)\b(?:[1-9]|[12]\d|3[0-5])\)(?!\d)", string.Empty);
        }
        body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
        body.Append(Titulo("B. INFORMACIÓN DE LA UNIDAD DE APRENDIZAJE"));
        foreach (var unit in data.Unidades.OrderBy(x => x.Orden))
        {
            body.Append(Titulo($"Unidad {unit.NumeroUnidad?.ToString() ?? string.Empty}: {unit.NombreUnidad}"));
            body.Append(Parrafo("Propósito esperado: " + (unit.PropositoEsperado ?? string.Empty)));
            body.Append(Tabla(new[] { new[] { "Horas saber", unit.HorasSaber?.ToString() ?? "" }, new[] { "Horas saber hacer", unit.HorasSaberHacer?.ToString() ?? "" }, new[] { "Horas totales", unit.HorasTotales?.ToString() ?? "" }, new[] { "Porcentaje", unit.PorcentajeUnidad?.ToString() ?? "" } }));
            body.Append(Titulo("C. SISTEMA DE EVALUACIÓN"));
            body.Append(Tabla(new[] { new[] { "Resultado de aprendizaje", "Evidencia", "Instrumento", "Ponderación" } }.Concat(unit.Evaluaciones.OrderBy(x => x.Orden).Select(x => new[] { x.ResultadoAprendizaje ?? "", x.EvidenciaAprendizaje ?? "", x.InstrumentoEvaluacion ?? "", x.Ponderacion?.ToString() ?? "" }))));
            body.Append(Titulo("D. SECUENCIA DIDÁCTICA"));
            foreach (var phase in new[] { FaseSecuencia.Apertura, FaseSecuencia.Desarrollo, FaseSecuencia.Cierre })
            {
                body.Append(Parrafo(phase.ToString().ToUpperInvariant()));
                body.Append(Tabla(new[] { new[] { "Método o técnica", "Actividad docente", "Actividad estudiante", "Evidencia", "Medios/materiales" } }
                    .Concat(ElementosDeFase(unit, phase).OrderBy(x => x.Orden).Select(x => new[]
                    {
                        x.MetodoTecnica?.ToString() ?? x.Estrategia?.ToString() ?? string.Empty,
                        x.ActividadDocente ?? string.Empty, x.ActividadEstudiante ?? string.Empty,
                        x.EvidenciaAprendizaje ?? string.Empty,
                        x.Recursos is { Count: > 0 } ? string.Join(", ", x.Recursos.OrderBy(r => r.Orden).Select(r => r.Nombre)) : x.MediosMateriales ?? string.Empty
                    }))));
            }
        }
        body.Append(Titulo("REFERENCIAS BIBLIOGRÁFICAS Y DIGITALES"));
        foreach (var reference in data.Referencias.OrderBy(x => x.Orden)) body.Append(Parrafo(reference.ReferenciaAPA));
        document.MainDocumentPart!.Document.Save();
    }

    private static IEnumerable<Plandi.Dto.Catalogos.SecuenciaPlaneacionEdicionDto> ElementosDeFase(Plandi.Dto.Catalogos.UnidadPlaneacionEdicionDto unidad, FaseSecuencia fase) => fase switch
    {
        FaseSecuencia.Apertura => unidad.Apertura ?? [],
        FaseSecuencia.Desarrollo => unidad.Desarrollo ?? [],
        FaseSecuencia.Cierre => unidad.Cierre ?? [],
        _ => []
    };

    private async Task<string> ConvertirAPdfAsync(string docx, string output, CancellationToken ct)
    {
        var executable = configuration["PdfConversion:LibreOfficePath"] ?? "soffice";
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
        start.ArgumentList.Add("--headless"); start.ArgumentList.Add("--convert-to"); start.ArgumentList.Add("pdf:writer_pdf_Export"); start.ArgumentList.Add("--outdir"); start.ArgumentList.Add(output); start.ArgumentList.Add(docx);
        try
        {
            using var process = Process.Start(start) ?? throw new PdfGenerationException("No fue posible iniciar LibreOffice.");
            await process.WaitForExitAsync(ct);
            if (process.ExitCode != 0) throw new PdfGenerationException($"LibreOffice no pudo convertir la plantilla a PDF: {await process.StandardError.ReadToEndAsync(ct)}");
        }
        catch (System.ComponentModel.Win32Exception) { throw new PdfGenerationException("LibreOffice no está instalado o PdfConversion:LibreOfficePath no es válido."); }
        var pdf = Path.ChangeExtension(docx, ".pdf");
        if (!File.Exists(pdf)) throw new PdfGenerationException("LibreOffice terminó sin producir un PDF.");
        return pdf;
    }

    private static Paragraph Titulo(string text) => new(new Run(new RunProperties(new Bold()), new Text(text)));
    private static Paragraph Parrafo(string text) => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    private static Table Tabla(IEnumerable<string[]> rows)
    {
        var table = new Table(new TableProperties(new TableBorders(new TopBorder { Val = BorderValues.Single, Size = 4 }, new BottomBorder { Val = BorderValues.Single, Size = 4 }, new LeftBorder { Val = BorderValues.Single, Size = 4 }, new RightBorder { Val = BorderValues.Single, Size = 4 }, new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 }, new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 })));
        foreach (var row in rows) table.Append(new TableRow(row.Select(cell => new TableCell(Parrafo(cell)))).ToArray());
        return table;
    }
}
