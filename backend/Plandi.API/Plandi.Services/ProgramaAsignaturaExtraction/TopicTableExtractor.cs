using System.Text;
using System.Text.RegularExpressions;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.ProgramaAsignaturaExtraction;

/// <summary>Reads the four-column topic table by PDF coordinates, never by character offsets.</summary>
public sealed class TopicTableExtractor
{
    // PdfPig can report words from one visual line a few points apart when the
    // source table mixes fonts. This stays well below the document line spacing.
    private const double RowTolerance = 5.5;

    public IReadOnlyDictionary<int, List<TemaProgramaExtraidoDto>> Extract(PdfProgramDocument document)
    {
        var result = new Dictionary<int, List<TemaProgramaExtraidoDto>>();
        var currentUnit = 0;
        foreach (var page in document.Pages)
        {
            var unit = FindUnit(page.Text);
            if (unit > 0) currentUnit = unit;
            if (currentUnit == 0 || !HasTopicHeader(page)) continue;

            var topics = ExtractPageTopics(page);
            if (topics.Count == 0) continue;
            if (!result.TryGetValue(currentUnit, out var unitTopics)) result[currentUnit] = unitTopics = [];
            unitTopics.AddRange(topics);
        }
        return result;
    }

    private static int FindUnit(string text)
    {
        var match = Regex.Match(text, @"Unidad\s+de\s+Aprendizaje\s+(?<number>[IVXLCDM]+)\.?");
        return match.Success ? RomanToInteger(match.Groups["number"].Value) : 0;
    }

    private static bool HasTopicHeader(PdfProgramPage page) => page.Words.Any(word => string.Equals(word.Text, "Temas", StringComparison.OrdinalIgnoreCase));

    private static List<TemaProgramaExtraidoDto> ExtractPageTopics(PdfProgramPage page)
    {
        var header = FindHeader(page);
        if (header is null) return [];
        var columns = FindColumns(page, header);
        if (columns is null) return [];

        var rows = BuildRows(page.Words)
            .Where(row => row.Y < header.Bottom - RowTolerance)
            .TakeWhile(row => !IsFooter(row.Text) && !StartsAnotherSection(row.Text))
            .ToList();
        var cells = rows.Select(row => new TableRow(row.Y,
            TextInColumn(row.Words, 0, columns.Value.TopicEnd),
            TextInColumn(row.Words, columns.Value.TopicEnd, columns.Value.SaberEnd),
            TextInColumn(row.Words, columns.Value.SaberEnd, columns.Value.SaberHacerEnd),
            TextInColumn(row.Words, columns.Value.SaberHacerEnd, double.MaxValue))).ToList();
        return BuildTopics(cells);
    }

    private static TableHeader? FindHeader(PdfProgramPage page)
    {
        var temas = page.Words.FirstOrDefault(word => string.Equals(word.Text, "Temas", StringComparison.OrdinalIgnoreCase));
        if (temas is null) return null;

        // This form uses a two-line table header: "Temas" is below the names of the
        // three dimensions. The horizontal column borders are therefore inferred from
        // every label in that small header block, not from a character position.
        var headerWords = page.Words
            .Where(word => word.Top >= temas.Top - 20 && word.Top <= temas.Top + 60)
            .ToList();
        var headerText = string.Join(" ", headerWords.OrderBy(word => word.Left).Select(word => word.Text));
        if (!Regex.IsMatch(headerText, @"\bSaber\b", RegexOptions.IgnoreCase) ||
            !Regex.IsMatch(headerText, @"\bHacer\b", RegexOptions.IgnoreCase) ||
            !Regex.IsMatch(headerText, @"\bSer\b", RegexOptions.IgnoreCase) ||
            !Regex.IsMatch(headerText, @"\bConvivir\b", RegexOptions.IgnoreCase)) return null;
        return new TableHeader(headerWords.Min(word => word.Top), headerWords);
    }

    private static (double TopicEnd, double SaberEnd, double SaberHacerEnd)? FindColumns(PdfProgramPage page, TableHeader header)
    {
        var words = header.Words;
        var temas = words.FirstOrDefault(word => string.Equals(word.Text, "Temas", StringComparison.OrdinalIgnoreCase));
        var dimensions = words.Where(word => string.Equals(word.Text, "Dimensión", StringComparison.OrdinalIgnoreCase))
            .OrderBy(word => word.Left).ToList();
        var conceptual = words.FirstOrDefault(word => string.Equals(word.Text, "Conceptual", StringComparison.OrdinalIgnoreCase));
        var actuational = words.FirstOrDefault(word => string.Equals(word.Text, "Actuacional", StringComparison.OrdinalIgnoreCase));
        var socioaffective = words.FirstOrDefault(word => string.Equals(word.Text, "Socioafectiva", StringComparison.OrdinalIgnoreCase));
        if (temas is null || dimensions.Count != 3 || conceptual is null || actuational is null || socioaffective is null) return null;

        var firstContentLeft = page.Words
            .Where(word => word.Top < header.Bottom - RowTolerance && word.Top > 100)
            .Min(word => word.Left);
        var topicCentre = (temas.Left + temas.Right) / 2;
        var conceptualCentre = (dimensions[0].Left + conceptual.Right) / 2;
        var actuationalCentre = (dimensions[1].Left + actuational.Right) / 2;
        var socioaffectiveCentre = (dimensions[2].Left + socioaffective.Right) / 2;

        // "Temas" and the three dimension labels are centred in their cells. From
        // those centres and the first table-content edge we recover the vertical
        // borders, independently of character counts or page-specific widths.
        return (2 * topicCentre - firstContentLeft,
            (conceptualCentre + actuationalCentre) / 2,
            (actuationalCentre + socioaffectiveCentre) / 2);
    }

    private static double Centre(PdfProgramWord word) => (word.Left + word.Right) / 2;

    private static List<TemaProgramaExtraidoDto> BuildTopics(IReadOnlyList<TableRow> rows)
    {
        var topics = new List<TemaProgramaExtraidoDto>();
        TemaBuilder? current = null;
        foreach (var row in rows)
        {
            if (!string.IsNullOrWhiteSpace(row.Topic))
            {
                if (current is not null && current.IsNameContinuation(row.Y))
                {
                    current.AppendName(row.Topic, row.Y);
                }
                else
                {
                    current = new TemaBuilder(row.Topic, row.Y);
                    topics.Add(current.Topic);
                }
            }
            if (current is null) continue;
            current.Append(row.Saber, Column.Saber);
            current.Append(row.SaberHacer, Column.SaberHacer);
            current.Append(row.SerConvivir, Column.SerConvivir);
        }
        return topics.Where(topic => !string.IsNullOrWhiteSpace(topic.Nombre)).ToList();
    }

    private static IEnumerable<(double Y, IReadOnlyList<PdfProgramWord> Words, string Text)> BuildRows(IReadOnlyList<PdfProgramWord> words)
    {
        var ordered = words.OrderByDescending(word => word.Top).ThenBy(word => word.Left).ToList();
        var rows = new List<List<PdfProgramWord>>();
        foreach (var word in ordered)
        {
            var row = rows.FirstOrDefault(candidate => Math.Abs(candidate[0].Top - word.Top) <= RowTolerance);
            if (row is null) rows.Add([word]); else row.Add(word);
        }
        return rows.Select(row =>
        {
            var orderedRow = row.OrderBy(word => word.Left).ToList();
            return (orderedRow[0].Top, (IReadOnlyList<PdfProgramWord>)orderedRow, string.Join(" ", orderedRow.Select(word => word.Text)));
        });
    }

    private static string TextInColumn(IReadOnlyList<PdfProgramWord> words, double left, double right)
    {
        var selected = words.Where(word =>
        {
            var center = (word.Left + word.Right) / 2;
            return center >= left && center < right;
        }).OrderBy(word => word.Left).Select(word => word.Text);
        return SectionReader.Clean(string.Join(" ", selected));
    }

    private static bool IsFooter(string text) => Regex.IsMatch(text, @"ELABOR(?:Ó|O):|REVIS(?:Ó|O):|APROB(?:Ó|O):|VIGENTE\s+A\s+PARTIR", RegexOptions.IgnoreCase);
    private static bool StartsAnotherSection(string text) => Regex.IsMatch(text, @"M[ée]todos\s+y\s+t[ée]cnicas|Proceso\s+Ense|Resultado\s+de\s+Aprendizaje|Formaci[óo]n\s+acad[ée]mica|Referencias", RegexOptions.IgnoreCase);
    private static int RomanToInteger(string value) => value.ToUpperInvariant() switch { "I" => 1, "II" => 2, "III" => 3, "IV" => 4, "V" => 5, _ => 0 };

    private sealed record TableHeader(double Bottom, IReadOnlyList<PdfProgramWord> Words);
    private sealed record TableRow(double Y, string Topic, string Saber, string SaberHacer, string SerConvivir);
    private enum Column { Saber, SaberHacer, SerConvivir }
    private sealed class TemaBuilder
    {
        public TemaProgramaExtraidoDto Topic { get; } = new();
        private double LastNameY { get; set; }
        public TemaBuilder(string name, double y)
        {
            Topic.Nombre = name;
            LastNameY = y;
        }
        public bool IsNameContinuation(double y) => LastNameY - y <= 24;
        public void AppendName(string value, double y)
        {
            Topic.Nombre = Join(Topic.Nombre, value);
            LastNameY = y;
        }
        public void Append(string value, Column column)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            switch (column)
            {
                case Column.Saber: Topic.Saber = Join(Topic.Saber, value); break;
                case Column.SaberHacer: Topic.SaberHacer = Join(Topic.SaberHacer, value); break;
                case Column.SerConvivir: Topic.SerConvivir = Join(Topic.SerConvivir, value); break;
            }
        }
        private static string Join(string? current, string next) => string.IsNullOrWhiteSpace(current) ? next : $"{current} {next}";
    }
}
