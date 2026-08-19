using System.Reflection;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Enums;
using Plandi.Services;
using Xunit;

namespace Plandi.Services.Tests;

public sealed class PlaneacionPdfServiceTests
{
    [Fact]
    public void Tabla_agrega_filas_completas_sin_reinsertar_celdas()
    {
        var method = typeof(PlaneacionPdfService).GetMethod("Tabla", BindingFlags.NonPublic | BindingFlags.Static)!;

        var table = (Table)method.Invoke(null, [new[] { new[] { "Encabezado", "Valor" } }])!;

        var row = Assert.Single(table.Elements<TableRow>());
        Assert.Equal(2, row.Elements<TableCell>().Count());
    }

    [Fact]
    public void ReemplazarFilas_crea_una_fila_valida_si_la_plantilla_solo_tiene_encabezados()
    {
        var table = new Table(
            new TableProperties(),
            new TableGrid(new GridColumn(), new GridColumn()),
            new TableRow(new TableCell(new Paragraph(new Run(new Text("Encabezado 1")))), new TableCell(new Paragraph(new Run(new Text("Encabezado 2"))))),
            new TableRow(new TableCell(new Paragraph(new Run(new Text("Encabezado 3")))), new TableCell(new Paragraph(new Run(new Text("Encabezado 4"))))));
        var method = typeof(PlaneacionPdfService).GetMethod("ReemplazarFilas", BindingFlags.NonPublic | BindingFlags.Static)!;

        method.Invoke(null, [table, 3, new[] { new[] { "Dato 1", "Dato 2" } }]);

        var row = table.Elements<TableRow>().ElementAt(2);
        Assert.Equal(3, table.Elements<TableRow>().Count());
        Assert.Equal("Dato 1", row.Elements<TableCell>().First().InnerText);
        Assert.Equal("Dato 2", row.Elements<TableCell>().ElementAt(1).InnerText);
    }

    [Fact]
    public void Completar_documento_llena_la_plantilla_activa_y_mantiene_openxml_valido()
    {
        var path = Path.Combine(Path.GetTempPath(), $"plandi-{Guid.NewGuid():N}.docx");
        try
        {
            var template = PlantillaActiva();
            using var original = WordprocessingDocument.Open(template, false);
            var sourceValidationErrors = new OpenXmlValidator().Validate(original).Count();
            File.Copy(template, path);

            var method = typeof(PlaneacionPdfService).GetMethod("CompletarDocumento", BindingFlags.NonPublic | BindingFlags.Static)!;
            method.Invoke(null, [path, new PlaneacionEdicionDto
            {
                Caratula = new CaratulaPlaneacionEdicionDto
                {
                    ProgramaEducativo = "Programa de prueba", Docentes = "Docente de prueba", Cuatrimestre = 9,
                    PeriodoEscolar = "Septiembre-Diciembre 2026", NombreAsignatura = "Asignatura de prueba", Grupos = "9A",
                    PropositoAsignatura = "Propósito de prueba", CompetenciaAsignatura = "Competencia de prueba", TipoCompetencia = "Específica",
                    Creditos = 5, Modalidad = "Escolarizada", HorasSaber = 10, HorasSaberHacer = 20, HorasTotales = 30, HorasSemana = 5
                },
                Unidades = [new UnidadPlaneacionEdicionDto
                {
                    NumeroUnidad = 1, NombreUnidad = "Unidad de prueba", PropositoEsperado = "Propósito de unidad", HorasSaber = 10, HorasSaberHacer = 20, HorasTotales = 30, PorcentajeUnidad = 100, Orden = 1,
                    Temas = [new TemaPlaneacionEdicionDto { Tema = "Tema de prueba", SaberConceptual = "Saber conceptual", SaberHacer = "Saber hacer", SaberSer = "Saber ser", Orden = 1 }],
                    Evaluaciones = [new EvaluacionPlaneacionEdicionDto { PeriodoSemanas = 3, ResultadoAprendizaje = "Resultado de prueba", EvidenciaAprendizaje = "Evidencia de prueba", TipoEvaluacion = TipoEvaluacion.Desempeno, Ponderacion = 100, InstrumentoEvaluacion = "Rúbrica", Orden = 1 }],
                    Apertura = [new SecuenciaPlaneacionEdicionDto { MetodoTecnica = MetodoTecnicaEnsenanzaAprendizaje.LluviaDeIdeas, ActividadDocente = "Actividad docente", ActividadEstudiante = "Actividad estudiante", EvidenciaAprendizaje = "Evidencia secuencia", MediosMateriales = "Pizarrón", Orden = 1 }]
                }],
                Referencias = [new ReferenciaPlaneacionEdicionDto { ReferenciaAPA = "Referencia de prueba", Orden = 1 }]
            }]);

            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length > 0);
            using var generated = WordprocessingDocument.Open(path, false);
            var errors = new OpenXmlValidator().Validate(generated).ToList();
            Assert.True(errors.Count <= sourceValidationErrors, "El llenado no debe introducir errores estructurales adicionales a los ya presentes en la plantilla activa.");
            Assert.IsType<SectionProperties>(generated.MainDocumentPart!.Document.Body!.LastChild);
            var text = generated.MainDocumentPart.Document.Body.InnerText;
            foreach (var expected in new[] { "Programa de prueba", "Docente de prueba", "9", "Septiembre-Diciembre 2026", "Asignatura de prueba", "9A", "Propósito de prueba", "5", "10", "Tema de prueba", "Saber conceptual", "Actividad docente", "Referencia de prueba" }) Assert.Contains(expected, text);
            Assert.DoesNotMatch(@"(?<!\d)\b(?:[1-9]|[12]\d|3[0-6])\)(?!\d)", text);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static string PlantillaActiva()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current is not null; current = current.Parent)
        {
            var directory = Path.Combine(current.FullName, "Plandi.API", "documentos", "plantillas-planeacion");
            var file = Directory.Exists(directory) ? Directory.GetFiles(directory, "*.docx").SingleOrDefault() : null;
            if (file is not null) return file;
        }
        throw new DirectoryNotFoundException("No se encontró la plantilla activa de planeación para la prueba de regresión.");
    }
}
