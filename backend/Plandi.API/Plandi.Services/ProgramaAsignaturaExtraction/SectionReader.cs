using System.Text.RegularExpressions;

namespace Plandi.Services.ProgramaAsignaturaExtraction;

internal static class SectionReader
{
    internal static string? Read(string text, string headerPattern, params string[] followingHeaders)
    {
        var next = followingHeaders.Length == 0 ? @"\z" : string.Join("|", followingHeaders.Select(pattern => $"(?:{pattern})"));
        var match = Regex.Match(text,
            $@"(?:^|\n|\s)(?:{headerPattern})\s*:?\s*(?<value>.*?)(?=(?:\s|\n)(?:{next})\s*:?|\z)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? Clean(match.Groups["value"].Value) : null;
    }

    internal static string Clean(string value) => Regex.Replace(value, @"\s+", " ").Trim();

    internal static List<(Match Header, string Body)> Split(string text, string headerPattern)
    {
        var headers = Regex.Matches(text, headerPattern, RegexOptions.IgnoreCase).Cast<Match>().ToList();
        var result = new List<(Match, string)>();
        for (var index = 0; index < headers.Count; index++)
        {
            var start = headers[index].Index + headers[index].Length;
            var end = index + 1 < headers.Count ? headers[index + 1].Index : text.Length;
            result.Add((headers[index], text[start..end]));
        }
        return result;
    }
}
