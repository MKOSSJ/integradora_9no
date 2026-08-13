using System.Globalization;
using System.Text.RegularExpressions;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.ProgramaAsignaturaExtraction;

public sealed class ProgramGeneralInfoExtractor
{
    private const string Proposito = "Prop(?:\\u00f3|o)sito\\s+de\\s+aprendizaje\\s+de\\s+la\\s+Asignatura";
    private const string Competencia = "Competencia\\s+a\\s+la\\s+que\\s+contribuye\\s+la\\s+asignatura";

    public void Populate(string text, ProgramaAsignaturaExtraidoDto result)
    {
        result.ProgramaEducativo = SectionReader.Read(text, "PROGRAMA\\s+EDUCATIVO", "EN\\s+COMPETENCIAS\\s+PROFESIONALES", "PROGRAMA\\s+DE\\s+ASIGNATURA") ?? string.Empty;
        result.NombreAsignatura = SectionReader.Read(text, "PROGRAMA\\s+DE\\s+ASIGNATURA", "CLAVE") ?? string.Empty;
        result.ClaveAsignatura = Regex.Match(text, @"CLAVE\s*:\s*(?<value>[^\s]+)", RegexOptions.IgnoreCase).Groups["value"].Value;
        result.Proposito = SectionReader.Read(text, Proposito, Competencia);
        result.Competencia = SectionReader.Read(text, Competencia, "Tipo\\s+de\\s+Competencia", "Unidad\\s+de\\s+Aprendizaje", "Funciones");

        var row = Regex.Match(text, @"(?<tipo>Específica|Especifica|Genérica|Generica)\s+(?<cuatrimestre>\d+)\s+(?<creditos>[\d.,]+)\s+(?<modalidad>\S+)\s+(?<semana>\d+)\s+(?<totales>\d+)", RegexOptions.IgnoreCase);
        if (!row.Success) return;

        result.TipoCompetencia = row.Groups["tipo"].Value;
        result.Cuatrimestre = int.Parse(row.Groups["cuatrimestre"].Value, CultureInfo.InvariantCulture);
        result.Creditos = decimal.Parse(row.Groups["creditos"].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
        result.Modalidad = row.Groups["modalidad"].Value;
        result.HorasSemana = int.Parse(row.Groups["semana"].Value, CultureInfo.InvariantCulture);
        result.HorasTotales = int.Parse(row.Groups["totales"].Value, CultureInfo.InvariantCulture);

        var unitTotals = Regex.Match(text, @"\bTotales\s+(?<saber>\d+)\s+(?<saberHacer>\d+)\s+(?<totales>\d+)", RegexOptions.IgnoreCase);
        if (!unitTotals.Success) return;
        result.HorasSaber = int.Parse(unitTotals.Groups["saber"].Value, CultureInfo.InvariantCulture);
        result.HorasSaberHacer = int.Parse(unitTotals.Groups["saberHacer"].Value, CultureInfo.InvariantCulture);
        result.HorasTotales = int.Parse(unitTotals.Groups["totales"].Value, CultureInfo.InvariantCulture);
    }
}
