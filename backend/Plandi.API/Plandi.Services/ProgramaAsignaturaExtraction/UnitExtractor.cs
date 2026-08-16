using System.Text.RegularExpressions;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.ProgramaAsignaturaExtraction;

public sealed class UnitExtractor
{
    private const string UnitHeader = @"Unidad\s+de\s+Aprendizaje\s+(?<number>[IVXLCDM]+)\.?";
    private static readonly string[] UnitHeaders = ["Prop(?:\\u00f3|o)sito\\s+esperado", "Tiempo\\s+Asignado"];

    public List<UnidadProgramaExtraidaDto> Extract(string text)
    {
        var units = new List<UnidadProgramaExtraidaDto>();
        foreach (var (header, body) in SectionReader.Split(text, UnitHeader))
        {
            var number = RomanToInteger(header.Groups["number"].Value);
            if (number == 0) continue;
            var unit = new UnidadProgramaExtraidaDto
            {
                Numero = number,
                Nombre = ReadName(body),
                Proposito = SectionReader.Read(body, UnitHeaders[0], UnitHeaders.Skip(1).ToArray()),
                TiempoAsignado = ExtractTime(body)
            };
            units.Add(unit);
        }
        return units;
    }

    private static string ReadName(string body)
    {
        var firstHeader = string.Join("|", UnitHeaders.Select(pattern => $"(?:{pattern})"));
        var match = Regex.Match(body, $@"^(?<value>.*?)(?=\s+(?:{firstHeader})\s*:?|\z)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return SectionReader.Clean(match.Groups["value"].Value);
    }

    private static TiempoAsignadoUnidadExtraidoDto ExtractTime(string body)
    {
        var timeText = SectionReader.Read(body, UnitHeaders[1]) ?? string.Empty;
        return new TiempoAsignadoUnidadExtraidoDto
        {
            HorasSaber = ReadHours(timeText, "Horas?\\s+del\\s+Saber(?!\\s+Hacer)"),
            HorasSaberHacer = ReadHours(timeText, "Horas?\\s+del\\s+Saber\\s+Hacer"),
            HorasTotales = ReadHours(timeText, "Horas?\\s+Totales?")
        };
    }

    private static int? ReadHours(string text, string label)
    {
        var match = Regex.Match(text, $@"(?:{label})\s*:?[\s]*(?<value>\d+)", RegexOptions.IgnoreCase);
        return match.Success ? int.Parse(match.Groups["value"].Value) : null;
    }

    private static int RomanToInteger(string value) => value.ToUpperInvariant() switch
    { "I" => 1, "II" => 2, "III" => 3, "IV" => 4, "V" => 5, _ => 0 };
}
