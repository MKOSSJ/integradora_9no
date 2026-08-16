using Plandi.Dto.Catalogos;

namespace Plandi.Services.ProgramaAsignaturaExtraction;

public sealed class ProgramaAsignaturaExtractionValidator
{
    public void Validate(ProgramaAsignaturaExtraidoDto program)
    {
        foreach (var unit in program.Unidades)
        {
            unit.Nombre = SectionReader.Clean(unit.Nombre);
            unit.Proposito = Clean(unit.Proposito);
            foreach (var topic in unit.Temas)
            {
                topic.Nombre = SectionReader.Clean(topic.Nombre);
                topic.Saber = Clean(topic.Saber);
                topic.SaberHacer = Clean(topic.SaberHacer);
                topic.SerConvivir = Clean(topic.SerConvivir);
            }
            unit.ProcesoEvaluacion.ResultadoAprendizaje = Clean(unit.ProcesoEvaluacion.ResultadoAprendizaje);
            unit.ProcesoEvaluacion.EvidenciaAprendizaje = Clean(unit.ProcesoEvaluacion.EvidenciaAprendizaje);
            unit.ProcesoEvaluacion.InstrumentosEvaluacion = Clean(unit.ProcesoEvaluacion.InstrumentosEvaluacion);
        }
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : SectionReader.Clean(value);
}
