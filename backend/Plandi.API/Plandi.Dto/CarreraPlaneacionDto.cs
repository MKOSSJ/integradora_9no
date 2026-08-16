namespace Plandi.Dto;

public class CarreraPlaneacionDto
{
    public string NombreCarrera { get; set; } = string.Empty;
    public List<PlaneacionDirectivoDto> PlaneacionesCarrera;
}