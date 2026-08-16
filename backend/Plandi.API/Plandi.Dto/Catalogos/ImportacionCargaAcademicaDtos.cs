namespace Plandi.Dto.Catalogos;

public class ImportacionCargaAcademicaResultadoDto
{
    public int TotalFilas { get; set; }
    public int Procesadas { get; set; }
    public int Insertadas { get; set; }
    public int Omitidas { get; set; }
    public List<ImportacionCargaAcademicaErrorDto> Errores { get; set; } = [];
}

public class ImportacionCargaAcademicaErrorDto
{
    public int Fila { get; set; }
    public string Campo { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
