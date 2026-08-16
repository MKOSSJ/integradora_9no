using System.Text.RegularExpressions;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.ProgramaAsignaturaExtraction;

/// <summary>Reads the three evaluation columns using their positions in the PDF table.</summary>
public sealed class EvaluationTableExtractor
{
    private const double RowTolerance = 5.5;

    public IReadOnlyDictionary<int, ProcesoEvaluacionUnidadExtraidoDto> Extract(PdfProgramDocument document)
    {
        var evaluations = new Dictionary<int, ProcesoEvaluacionUnidadExtraidoDto>();
        var currentUnit = 0;
        foreach (var page in document.Pages)
        {
            var unit = FindUnit(page.Text);
            if (unit > 0) currentUnit = unit;
            if (currentUnit == 0 || !TryFindHeader(page, out var header)) continue;
            evaluations[currentUnit] = Read(page, header);
        }
        return evaluations;
    }

    private static int FindUnit(string text)
    {
        var match = Regex.Match(text, @"Unidad\s+de\s+Aprendizaje\s+(?<number>[IVXLCDM]+)\.?");
        return match.Success ? match.Groups["number"].Value.ToUpperInvariant() switch
        { "I" => 1, "II" => 2, "III" => 3, "IV" => 4, "V" => 5, _ => 0 } : 0;
    }

    private static bool TryFindHeader(PdfProgramPage page, out Header header)
    {
        foreach (var row in BuildRows(page.Words))
        {
            var words = page.Words.Where(word => Math.Abs(word.Top - row.Y) <= 8).ToList();
            var text = string.Join(" ", words.Select(word => word.Text));
            if (!text.Contains("Resultado", StringComparison.OrdinalIgnoreCase) ||
                !text.Contains("Evidencia", StringComparison.OrdinalIgnoreCase) ||
                !text.Contains("Instrumentos", StringComparison.OrdinalIgnoreCase)) continue;
            var result = words.First(word => word.Text.Equals("Resultado", StringComparison.OrdinalIgnoreCase));
            var evidence = words.First(word => word.Text.Equals("Evidencia", StringComparison.OrdinalIgnoreCase));
            var instruments = words.First(word => word.Text.Equals("Instrumentos", StringComparison.OrdinalIgnoreCase));
            var resultCentre = Centre(words.Where(word => word.Left >= result.Left && word.Left < evidence.Left));
            var evidenceCentre = Centre(words.Where(word => word.Left >= evidence.Left && word.Left < instruments.Left));
            var instrumentsCentre = Centre(words.Where(word => word.Left >= instruments.Left));
            header = new Header(words.Min(word => word.Top),
                (resultCentre + evidenceCentre) / 2,
                (evidenceCentre + instrumentsCentre) / 2);
            return true;
        }
        header = default;
        return false;
    }

    private static ProcesoEvaluacionUnidadExtraidoDto Read(PdfProgramPage page, Header header)
    {
        var columns = new[] { new List<string>(), new List<string>(), new List<string>() };
        foreach (var row in BuildRows(page.Words)
            .Where(row => row.Y < header.Bottom - RowTolerance)
            .TakeWhile(row => !IsFooter(row.Words) && !StartsNextSection(row.Words)))
        foreach (var word in row.Words.OrderBy(word => word.Left))
        {
            var centre = (word.Left + word.Right) / 2;
            columns[centre < header.ResultEnd ? 0 : centre < header.EvidenceEnd ? 1 : 2].Add(word.Text);
        }
        return new ProcesoEvaluacionUnidadExtraidoDto
        {
            ResultadoAprendizaje = Clean(columns[0]),
            EvidenciaAprendizaje = Clean(columns[1]),
            InstrumentosEvaluacion = Clean(columns[2])
        };
    }

    private static List<(double Y, IReadOnlyList<PdfProgramWord> Words)> BuildRows(IReadOnlyList<PdfProgramWord> words)
    {
        var rows = new List<List<PdfProgramWord>>();
        foreach (var word in words.OrderByDescending(word => word.Top).ThenBy(word => word.Left))
        {
            var row = rows.FirstOrDefault(candidate => Math.Abs(candidate[0].Top - word.Top) <= RowTolerance);
            if (row is null) rows.Add([word]); else row.Add(word);
        }
        return rows.Select(row => (row[0].Top, (IReadOnlyList<PdfProgramWord>)row.OrderBy(word => word.Left).ToList())).ToList();
    }

    private static bool IsFooter(IReadOnlyList<PdfProgramWord> words) => words.Any(word => word.Text.StartsWith("ELABOR", StringComparison.OrdinalIgnoreCase) || word.Text.StartsWith("APROB", StringComparison.OrdinalIgnoreCase));
    private static bool StartsNextSection(IReadOnlyList<PdfProgramWord> words)
    {
        var text = string.Join(" ", words.Select(word => word.Text));
        return text.Contains("Formación académica", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("Perfil idóneo", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("Referencias", StringComparison.OrdinalIgnoreCase);
    }
    private static double Centre(IEnumerable<PdfProgramWord> words)
    {
        var labels = words.ToList();
        return (labels.Min(word => word.Left) + labels.Max(word => word.Right)) / 2;
    }
    private static string? Clean(IEnumerable<string> values)
    {
        var value = SectionReader.Clean(string.Join(" ", values));
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private readonly record struct Header(double Bottom, double ResultEnd, double EvidenceEnd);
}
