using Plandi.Dto.Catalogos;

namespace Plandi.Services.ProgramaAsignaturaExtraction;

public sealed class ProgramaAsignaturaExtractor(
    ProgramGeneralInfoExtractor generalInfoExtractor,
    UnitExtractor unitExtractor,
    TopicTableExtractor topicTableExtractor,
    EvaluationTableExtractor evaluationTableExtractor,
    ReferencesExtractor referencesExtractor,
    ProgramaAsignaturaExtractionValidator validator)
{
    public ProgramaAsignaturaExtraidoDto Extract(PdfProgramDocument document)
    {
        var rawText = document.RawText;
        var result = new ProgramaAsignaturaExtraidoDto();
        generalInfoExtractor.Populate(rawText, result);
        result.Funciones = SectionReader.Read(rawText, "Funciones", "Capacidades", "Criterios\\s+de\\s+Desempe(?:\\u00f1|n)o", "Perfil\\s+Id(?:\\u00f3|o)neo");
        result.Capacidades = SectionReader.Read(rawText, "Capacidades", "Criterios\\s+de\\s+Desempe(?:\\u00f1|n)o", "Perfil\\s+Id(?:\\u00f3|o)neo", "Unidad\\s+de\\s+Aprendizaje");
        result.CriteriosDesempeno = SectionReader.Read(rawText, "Criterios\\s+de\\s+Desempe(?:\\u00f1|n)o", "Perfil\\s+Id(?:\\u00f3|o)neo", "Unidad\\s+de\\s+Aprendizaje", "Referencias?");
        result.PerfilIdoneoDocente = SectionReader.Read(rawText, "Perfil\\s+Id(?:\\u00f3|o)neo(?:\\s+del)?\\s+Docente", "Referencias?", "Unidad\\s+de\\s+Aprendizaje");
        result.Unidades = unitExtractor.Extract(rawText);
        var topicsByUnit = topicTableExtractor.Extract(document);
        var evaluationsByUnit = evaluationTableExtractor.Extract(document);
        foreach (var unit in result.Unidades)
        {
            unit.Temas = topicsByUnit.TryGetValue(unit.Numero, out var topics) ? topics : [];
            unit.ProcesoEvaluacion = evaluationsByUnit.TryGetValue(unit.Numero, out var evaluation) ? evaluation : new();
        }
        referencesExtractor.Populate(document, result);
        validator.Validate(result);
        return result;
    }
}
