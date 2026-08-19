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
        var tables = body.Elements<Table>().ToList();
        if (tables.Count < 13) throw new PdfGenerationException("La plantilla DOCX no contiene la estructura de tablas requerida para la planeación.");

        CompletarCaratula(tables, c);
        var unitTemplates = tables.Skip(5).Take(6).ToArray();
        var referencias = tables[11];
        var resumenUnidades = tables[12];
        var units = data.Unidades.OrderBy(unit => unit.Orden).ToList();

        for (var index = 0; index < units.Count; index++)
        {
            var unitTables = index == 0
                ? unitTemplates
                : unitTemplates.Select(table => (Table)table.CloneNode(true)).ToArray();
            foreach (var table in unitTables) ColocarTablaEnFlujo(table);
            if (index > 0)
            {
                body.InsertBefore(CrearSeparadorEntreUnidades(), referencias);
                foreach (var table in unitTables) body.InsertBefore(table, referencias);
            }
            CompletarUnidad(unitTables, units[index]);
        }

        if (units.Count == 0)
            foreach (var table in unitTemplates) LimpiarSeccionUnidad(table);

        CompletarReferencias(referencias, data.Referencias.OrderBy(reference => reference.Orden));
        CompletarResumenUnidades(resumenUnidades, units);
        LimpiarMarcadoresDePlantilla(body);
        document.MainDocumentPart!.Document.Save();
    }

    private static void CompletarCaratula(IReadOnlyList<Table> tables, Plandi.Dto.Catalogos.CaratulaPlaneacionEdicionDto c)
    {
        foreach (var row in tables[0].Elements<TableRow>()) EvitarDivisionDeFila(row);
        EvitarDivisionDeFila(tables[3].Elements<TableRow>().First());
        EvitarDivisionDeFila(tables[4].Elements<TableRow>().First());
        EstablecerCelda(tables[0], 0, 1, c.ProgramaEducativo); EstablecerCelda(tables[0], 0, 3, c.Docentes);
        EstablecerCelda(tables[0], 1, 1, c.Cuatrimestre?.ToString()); EstablecerCelda(tables[0], 1, 3, c.PeriodoEscolar);
        EstablecerCelda(tables[0], 2, 1, c.NombreAsignatura); EstablecerCelda(tables[0], 2, 3, c.Grupos);
        EstablecerCelda(tables[1], 0, 1, c.PropositoAsignatura); EstablecerCelda(tables[2], 0, 1, c.CompetenciaAsignatura);
        EstablecerCelda(tables[3], 0, 1, c.TipoCompetencia); EstablecerCelda(tables[3], 0, 3, c.Creditos?.ToString()); EstablecerCelda(tables[3], 0, 5, c.Modalidad);
        EstablecerCelda(tables[4], 0, 1, c.HorasSaber?.ToString()); EstablecerCelda(tables[4], 0, 3, c.HorasSaberHacer?.ToString());
        EstablecerCelda(tables[4], 0, 5, c.HorasTotales?.ToString()); EstablecerCelda(tables[4], 0, 7, c.HorasSemana?.ToString());
    }

    private static void CompletarUnidad(IReadOnlyList<Table> tables, Plandi.Dto.Catalogos.UnidadPlaneacionEdicionDto unit)
    {
        var informacion = tables[0];
        EvitarDivisionDeFila(informacion.Elements<TableRow>().ElementAt(0));
        EvitarDivisionDeFila(informacion.Elements<TableRow>().ElementAt(2));
        EvitarDivisionDeFila(informacion.Elements<TableRow>().ElementAt(3));
        EvitarDivisionDeFila(tables[1].Elements<TableRow>().First());
        foreach (var row in tables[2].Elements<TableRow>().Take(3)) EvitarDivisionDeFila(row);
        EstablecerCelda(informacion, 0, 1, unit.NombreUnidad); EstablecerCelda(informacion, 1, 1, unit.PropositoEsperado);
        EstablecerCelda(informacion, 3, 0, unit.HorasSaber?.ToString()); EstablecerCelda(informacion, 3, 1, unit.HorasSaberHacer?.ToString());
        EstablecerCelda(informacion, 3, 2, unit.HorasTotales?.ToString()); EstablecerCelda(informacion, 3, 3, unit.PorcentajeUnidad?.ToString());

        ReemplazarFilas(tables[1], 1, unit.Temas.OrderBy(topic => topic.Orden).Select(topic => new[] { topic.Tema, topic.SaberConceptual ?? string.Empty, topic.SaberHacer ?? string.Empty, topic.SaberSer ?? string.Empty }));
        EstablecerCelda(tables[2], 1, 1, unit.Evaluaciones.OrderBy(evaluation => evaluation.Orden).FirstOrDefault()?.PeriodoSemanas?.ToString());
        ReemplazarFilas(tables[2], 3, unit.Evaluaciones.OrderBy(evaluation => evaluation.Orden).Select(evaluation => new[]
        {
            evaluation.ResultadoAprendizaje ?? string.Empty, evaluation.EvidenciaAprendizaje ?? string.Empty,
            Etiqueta(evaluation.TipoEvaluacion), evaluation.Ponderacion?.ToString() ?? string.Empty, evaluation.InstrumentoEvaluacion ?? string.Empty
        }));

        CompletarFase(tables[3], "APERTURA", unit.Apertura ?? [], FaseSecuencia.Apertura);
        CompletarFase(tables[4], "DESARROLLO", unit.Desarrollo ?? [], FaseSecuencia.Desarrollo);
        CompletarFase(tables[5], "CIERRE", unit.Cierre ?? [], FaseSecuencia.Cierre);
    }

    private static void CompletarFase(Table table, string titulo, IEnumerable<Plandi.Dto.Catalogos.SecuenciaPlaneacionEdicionDto> sequences, FaseSecuencia fase)
    {
        foreach (var row in table.Elements<TableRow>().Take(3)) EvitarDivisionDeFila(row);
        EstablecerCelda(table, 0, 0, titulo);
        ReemplazarFilas(table, 3, sequences.OrderBy(sequence => sequence.Orden).Select(sequence => new[]
        {
            Etiqueta(sequence.MetodoTecnica) ?? EtiquetaEstrategia(fase, sequence.Estrategia), sequence.ActividadDocente ?? string.Empty,
            sequence.ActividadEstudiante ?? string.Empty, sequence.EvidenciaAprendizaje ?? string.Empty,
            sequence.Recursos is { Count: > 0 } ? string.Join(", ", sequence.Recursos.OrderBy(resource => resource.Orden).Select(resource => resource.Nombre)) : sequence.MediosMateriales ?? string.Empty
        }));
    }

    private static void CompletarReferencias(Table table, IEnumerable<Plandi.Dto.Catalogos.ReferenciaPlaneacionEdicionDto> referencias) =>
        EstablecerLineas(Celda(table, 1, 0), referencias.Select(reference => reference.ReferenciaAPA));

    private static void CompletarResumenUnidades(Table table, IReadOnlyCollection<Plandi.Dto.Catalogos.UnidadPlaneacionEdicionDto> units)
    {
        var rows = table.Elements<TableRow>().ToList();
        if (rows.Count < 3) throw new PdfGenerationException("La tabla de resumen de unidades de la plantilla no tiene filas suficientes.");
        EvitarDivisionDeFila(rows[0]);
        var template = rows[2]; var totalTemplate = rows[^1];
        foreach (var row in rows.Skip(1).ToList()) row.Remove();
        foreach (var unit in units)
        {
            var row = (TableRow)template.CloneNode(true);
            EstablecerCeldas(row, [unit.NombreUnidad, unit.HorasSaber?.ToString() ?? string.Empty, unit.HorasSaberHacer?.ToString() ?? string.Empty, unit.HorasTotales?.ToString() ?? string.Empty]);
            table.Append(row);
        }
        var total = (TableRow)totalTemplate.CloneNode(true);
        EstablecerCeldas(total, ["Totales", Sumar(units.Select(unit => unit.HorasSaber)), Sumar(units.Select(unit => unit.HorasSaberHacer)), Sumar(units.Select(unit => unit.HorasTotales))]);
        table.Append(total);
    }

    private static void LimpiarSeccionUnidad(Table table)
    {
        foreach (var cell in table.Descendants<TableCell>()) ReemplazarMarcadores(cell);
    }

    private static void ReemplazarFilas(Table table, int dataRowIndex, IEnumerable<string[]> values)
    {
        var rows = table.Elements<TableRow>().ToList();
        var valuesRows = values.ToList();
        var template = rows.ElementAtOrDefault(dataRowIndex);
        if (template is not null)
            foreach (var row in rows.Skip(dataRowIndex).ToList()) row.Remove();
        else if (valuesRows.Count > 0)
            template = CrearFilaDeDatos(table, valuesRows.Max(row => row.Length));
        else
            return;

        foreach (var valuesRow in valuesRows)
        {
            var row = (TableRow)template.CloneNode(true);
            EstablecerCeldas(row, valuesRow);
            table.Append(row);
        }
    }

    private static TableRow CrearFilaDeDatos(Table table, int columnCount)
    {
        var gridColumns = table.GetFirstChild<TableGrid>()?.Elements<GridColumn>().Count() ?? 0;
        var cells = Enumerable.Range(0, Math.Max(1, Math.Max(gridColumns, columnCount)))
            .Select(_ => CrearCeldaDeDatos())
            .ToArray();
        return new TableRow(cells);
    }

    private static TableCell CrearCeldaDeDatos()
    {
        var cell = new TableCell(new TableCellProperties(), new Paragraph(new Run()));
        AsegurarBordesDeCelda(cell);
        return cell;
    }

    private static void EstablecerCeldas(TableRow row, IReadOnlyList<string> values)
    {
        var cells = row.Elements<TableCell>().ToList();
        for (var index = 0; index < cells.Count; index++)
        {
            AsegurarBordesDeCelda(cells[index]);
            EstablecerTexto(cells[index], index < values.Count ? values[index] : string.Empty);
        }
    }

    private static void EstablecerCelda(Table table, int row, int cell, string? value) => EstablecerTexto(Celda(table, row, cell), value ?? string.Empty);
    private static TableCell Celda(Table table, int row, int cell)
    {
        var targetRow = table.Elements<TableRow>().ElementAtOrDefault(row) ?? throw new PdfGenerationException("La plantilla no contiene la fila esperada.");
        return targetRow.Elements<TableCell>().ElementAtOrDefault(cell) ?? throw new PdfGenerationException("La plantilla no contiene la celda esperada.");
    }

    private static void EstablecerTexto(TableCell cell, string value)
    {
        var texts = cell.Descendants<Text>().ToList();
        if (texts.Count == 0)
        {
            cell.Append(new Paragraph(CrearRunDinamico(value)));
            return;
        }
        texts[0].Text = value;
        AplicarFormatoDinamico(texts[0]);
        foreach (var text in texts.Skip(1))
        {
            text.Text = string.Empty;
            AplicarFormatoDinamico(text);
        }
    }

    private static void EstablecerLineas(TableCell cell, IEnumerable<string> lines)
    {
        var values = lines.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        var template = cell.Elements<Paragraph>().FirstOrDefault();
        if (template is null) { EstablecerTexto(cell, string.Join(Environment.NewLine, values)); return; }
        foreach (var paragraph in cell.Elements<Paragraph>().ToList()) paragraph.Remove();
        if (values.Count == 0) { cell.Append((Paragraph)template.CloneNode(true)); EstablecerTexto(cell, string.Empty); return; }
        foreach (var value in values)
        {
            var paragraph = (Paragraph)template.CloneNode(true);
            cell.Append(paragraph);
            var texts = paragraph.Descendants<Text>().ToList();
            if (texts.Count == 0) paragraph.Append(CrearRunDinamico(value));
            else
            {
                texts[0].Text = value;
                AplicarFormatoDinamico(texts[0]);
                foreach (var text in texts.Skip(1))
                {
                    text.Text = string.Empty;
                    AplicarFormatoDinamico(text);
                }
            }
        }
    }

    private static Run CrearRunDinamico(string value) => new(new RunProperties(new Color { Val = "000000" }), new Text(value) { Space = SpaceProcessingModeValues.Preserve });

    private static void AplicarFormatoDinamico(Text text)
    {
        var run = text.Ancestors<Run>().FirstOrDefault();
        if (run is null) return;
        var properties = run.RunProperties ?? run.PrependChild(new RunProperties());
        properties.Color = new Color { Val = "000000" };
    }

    private static void EvitarDivisionDeFila(TableRow row)
    {
        var properties = row.TableRowProperties ?? row.PrependChild(new TableRowProperties());
        properties.RemoveAllChildren<CantSplit>();
        properties.Append(new CantSplit());
    }

    private static void ColocarTablaEnFlujo(Table table) =>
        table.GetFirstChild<TableProperties>()?.RemoveAllChildren<TablePositionProperties>();

    private static void AsegurarBordesDeCelda(TableCell cell)
    {
        var properties = cell.TableCellProperties ?? cell.PrependChild(new TableCellProperties());
        var borders = properties.TableCellBorders ?? properties.AppendChild(new TableCellBorders());
        borders.TopBorder ??= BordeVisible<TopBorder>();
        borders.BottomBorder ??= BordeVisible<BottomBorder>();
        borders.LeftBorder ??= BordeVisible<LeftBorder>();
        borders.RightBorder ??= BordeVisible<RightBorder>();
    }

    private static TBorder BordeVisible<TBorder>() where TBorder : BorderType, new() => new()
    {
        Val = BorderValues.Single,
        Size = 4,
        Color = "000000"
    };

    private static Paragraph CrearSeparadorEntreUnidades() => new(
        new ParagraphProperties(new SpacingBetweenLines { Before = "120", After = "0" }),
        new Run(new Break()));

    private static void ReemplazarMarcadores(TableCell cell)
    {
        foreach (var text in cell.Descendants<Text>()) text.Text = Regex.Replace(text.Text, @"(?<!\d)\b(?:[1-9]|[12]\d|3[0-6])\)(?!\d)", string.Empty);
    }

    private static void LimpiarMarcadoresDePlantilla(Body body)
    {
        foreach (var text in body.Descendants<Text>()) text.Text = Regex.Replace(text.Text, @"(?<!\d)\b(?:[1-9]|[12]\d|3[0-6])\)(?!\d)", string.Empty);
    }

    private static string Sumar(IEnumerable<int?> values) => values.Any(value => value.HasValue) ? values.Sum(value => value ?? 0).ToString() : string.Empty;
    private static string Etiqueta(TipoEvaluacion? value) => value is null ? string.Empty : Etiqueta((Enum)value);
    private static string? Etiqueta(MetodoTecnicaEnsenanzaAprendizaje? value) => value is null ? null : Etiqueta((Enum)value);
    private static string EtiquetaEstrategia(FaseSecuencia phase, int? value) => value is null ? string.Empty : phase switch
    {
        FaseSecuencia.Apertura when Enum.IsDefined(typeof(EstrategiaApertura), value.Value) => Etiqueta((Enum)(EstrategiaApertura)value.Value),
        FaseSecuencia.Desarrollo when Enum.IsDefined(typeof(EstrategiaDesarrollo), value.Value) => Etiqueta((Enum)(EstrategiaDesarrollo)value.Value),
        FaseSecuencia.Cierre when Enum.IsDefined(typeof(EstrategiaCierre), value.Value) => Etiqueta((Enum)(EstrategiaCierre)value.Value),
        _ => string.Empty
    };
    private static string Etiqueta(Enum value) => Regex.Replace(value.ToString(), "(?<=[a-záéíóú])(?=[A-ZÁÉÍÓÚ])", " ")
        .Replace("Analisis", "Análisis", StringComparison.Ordinal).Replace("Tecnica", "Técnica", StringComparison.Ordinal).Replace("Desempeno", "Desempeño", StringComparison.Ordinal).Replace("Practica", "Práctica", StringComparison.Ordinal).Replace("Investigacion", "Investigación", StringComparison.Ordinal).Replace("CuestionarioReflexion", "Cuestionario Reflexión", StringComparison.Ordinal);

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
    private static void AgregarAlCuerpo(Body body, OpenXmlElement element)
    {
        var sectionProperties = body.Elements<SectionProperties>().LastOrDefault();
        if (sectionProperties is null) body.Append(element);
        else body.InsertBefore(element, sectionProperties);
    }
    private static Table Tabla(IEnumerable<string[]> rows)
    {
        var filas = rows.Select(row => row.ToArray()).ToList();
        var columnas = filas.Select(row => row.Length).DefaultIfEmpty(1).Max();
        var table = new Table(
            new TableProperties(new TableBorders(new TopBorder { Val = BorderValues.Single, Size = 4 }, new LeftBorder { Val = BorderValues.Single, Size = 4 }, new BottomBorder { Val = BorderValues.Single, Size = 4 }, new RightBorder { Val = BorderValues.Single, Size = 4 }, new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 }, new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 })),
            new TableGrid(Enumerable.Range(0, columnas).Select(_ => new GridColumn()).ToArray()));
        foreach (var row in filas) table.Append(new TableRow(row.Select(cell => new TableCell(Parrafo(cell))).ToArray()));
        return table;
    }
}
