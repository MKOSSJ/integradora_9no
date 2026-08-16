using System.Text.RegularExpressions;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.ProgramaAsignaturaExtraction;

/// <summary>Reads the reference tables from their PDF columns instead of flattened page text.</summary>
public sealed class ReferencesExtractor
{
    private const double RowTolerance = 5.5;

    public void Populate(PdfProgramDocument document, ProgramaAsignaturaExtraidoDto result)
    {
        result.ReferenciasBibliograficas = ExtractBibliographic(document);
        result.ReferenciasDigitales = ExtractDigital(document);
    }

    private static List<ReferenciaBibliograficaExtraidaDto> ExtractBibliographic(PdfProgramDocument document)
    {
        var pageIndex = FindPage(document, "Referencias bibliográficas");
        if (pageIndex < 0) return [];
        var header = FindHeader(document.Pages[pageIndex], "Autor", "Año", "Título", "Lugar", "Editorial", "ISBN");
        if (header is null) return [];
        var labels = header.Words;
        var limits = new[]
        {
            Find(labels, "Año").Left - 20,
            Find(labels, "Título").Left - 20,
            Find(labels, "Lugar").Left - 10,
            Find(labels, "Editorial").Left - 25,
            Find(labels, "ISBN").Left - 25
        };

        var entries = new List<ReferenceRow>();
        for (var index = pageIndex; index < document.Pages.Count; index++)
        {
            var page = document.Pages[index];
            var stop = page.Text.IndexOf("Referencias digitales", StringComparison.OrdinalIgnoreCase);
            foreach (var row in Rows(page, index == pageIndex ? header.Bottom : double.MaxValue, stop >= 0))
            {
                var cells = Cells(row.Words, limits);
                if (Regex.IsMatch(cells[1], @"^(?:19|20)\d{2}$")) entries.Add(new ReferenceRow(cells, row.Y));
                else if (entries.Count > 0) entries[^1].Append(cells);
            }
            if (stop >= 0) break;
        }
        return entries.Select(entry => new ReferenciaBibliograficaExtraidaDto
        {
            Autor = entry.Values[0], Anio = entry.Values[1], Titulo = entry.Values[2],
            LugarPublicacion = entry.Values[3], Editorial = entry.Values[4], Isbn = entry.Values[5]
        }).ToList();
    }

    private static List<ReferenciaDigitalExtraidaDto> ExtractDigital(PdfProgramDocument document)
    {
        var pageIndex = FindPage(document, "Referencias digitales");
        if (pageIndex < 0) return [];
        var header = FindHeader(document.Pages[pageIndex], "Autor", "Fecha", "Título", "Vínculo");
        if (header is null) return [];
        var labels = header.Words;
        var limits = new[]
        {
            Find(labels, "Fecha").Left - 20,
            Find(labels, "Título").Left - 90,
            Find(labels, "Vínculo").Left - 55
        };

        var entries = new List<ReferenceRow>();
        foreach (var row in Rows(document.Pages[pageIndex], header.Bottom, false))
        {
            var cells = Cells(row.Words, limits);
            var startsEntry = !string.IsNullOrWhiteSpace(cells[0]) &&
                (!string.IsNullOrWhiteSpace(cells[1]) || entries.Count == 0 || entries[^1].Y - row.Y > 22);
            if (startsEntry) entries.Add(new ReferenceRow(cells, row.Y));
            else if (entries.Count > 0) entries[^1].Append(cells);
        }
        return entries.Select(entry => new ReferenciaDigitalExtraidaDto
        {
            Autor = entry.Values[0], FechaRecuperacion = entry.Values[1],
            Titulo = entry.Values[2], Vinculo = NormalizeUrl(entry.Values[3])
        }).ToList();
    }

    private static int FindPage(PdfProgramDocument document, string text) => document.Pages.ToList()
        .FindIndex(page => page.Text.Contains(text, StringComparison.OrdinalIgnoreCase));

    private static TableHeader? FindHeader(PdfProgramPage page, params string[] required)
    {
        var rows = BuildRows(page.Words);
        foreach (var row in rows)
        {
            var words = rows.Where(candidate => candidate.Y >= row.Y - 8 && candidate.Y <= row.Y + 8)
                .SelectMany(candidate => candidate.Words).ToList();
            var text = string.Join(" ", words.Select(word => word.Text));
            if (required.All(value => text.Contains(value, StringComparison.OrdinalIgnoreCase)))
                return new TableHeader(words.Min(word => word.Top), words);
        }
        return null;
    }

    private static PdfProgramWord Find(IReadOnlyList<PdfProgramWord> words, string value) => words
        .First(word => string.Equals(word.Text, value, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<(double Y, IReadOnlyList<PdfProgramWord> Words)> Rows(PdfProgramPage page, double headerBottom, bool stopAtDigital)
        => BuildRows(page.Words)
            .Where(row => row.Y < headerBottom - RowTolerance)
            .TakeWhile(row => !IsFooter(row.Words) && !(stopAtDigital && row.Words.Any(word => word.Text.Equals("Referencias", StringComparison.OrdinalIgnoreCase))));

    private static List<string> Cells(IReadOnlyList<PdfProgramWord> words, IReadOnlyList<double> limits)
    {
        var cells = Enumerable.Range(0, limits.Count + 1).Select(_ => new List<PdfProgramWord>()).ToList();
        foreach (var word in words)
        {
            var centre = (word.Left + word.Right) / 2;
            var index = 0;
            while (index < limits.Count && centre >= limits[index]) index++;
            cells[index].Add(word);
        }
        return cells.Select(cell => SectionReader.Clean(string.Join(" ", cell.OrderBy(word => word.Left).Select(word => word.Text)))).ToList();
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

    private static bool IsFooter(IReadOnlyList<PdfProgramWord> words) => words.Any(word =>
        word.Text.StartsWith("ELABOR", StringComparison.OrdinalIgnoreCase) || word.Text.StartsWith("APROB", StringComparison.OrdinalIgnoreCase));

    private static string NormalizeUrl(string value) => Regex.Replace(value, @"\s+", string.Empty);

    private sealed record TableHeader(double Bottom, IReadOnlyList<PdfProgramWord> Words);
    private sealed class ReferenceRow(List<string> values, double y)
    {
        public List<string> Values { get; } = values;
        public double Y { get; } = y;
        public void Append(IReadOnlyList<string> cells)
        {
            for (var index = 0; index < Values.Count; index++)
                if (!string.IsNullOrWhiteSpace(cells[index]))
                    Values[index] = string.IsNullOrWhiteSpace(Values[index]) ? cells[index] : $"{Values[index]} {cells[index]}";
        }
    }
}
