using UglyToad.PdfPig;

namespace Plandi.Services.ProgramaAsignaturaExtraction;

public interface IPdfTextExtractor
{
    string Extract(Stream pdfStream);
    PdfProgramDocument ExtractDocument(Stream pdfStream);
}

public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    public string Extract(Stream pdfStream)
        => ExtractDocument(pdfStream).RawText;

    public PdfProgramDocument ExtractDocument(Stream pdfStream)
    {
        pdfStream.Position = 0;
        using var pdf = PdfDocument.Open(pdfStream);
        var pages = pdf.GetPages().Select((page, index) => new PdfProgramPage(
            index + 1,
            page.Text,
            page.GetWords().Select(word => new PdfProgramWord(
                word.Text,
                word.BoundingBox.Left,
                word.BoundingBox.Right,
                word.BoundingBox.Bottom,
                word.BoundingBox.Top)).ToList())).ToList();
        return new PdfProgramDocument(string.Join("\n", pages.Select(page => page.Text)), pages);
    }
}

public sealed record PdfProgramDocument(string RawText, IReadOnlyList<PdfProgramPage> Pages);
public sealed record PdfProgramPage(int Number, string Text, IReadOnlyList<PdfProgramWord> Words);
public sealed record PdfProgramWord(string Text, double Left, double Right, double Bottom, double Top);
