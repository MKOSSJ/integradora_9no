using System.ComponentModel.DataAnnotations;

namespace Plandi.Dto.Catalogos;

public sealed class RegistrarDispositivoDto
{
    [Required(ErrorMessage = "El token FCM es obligatorio.")]
    [MaxLength(500, ErrorMessage = "El token FCM no puede superar los 500 caracteres.")]
    public string FcmToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de dispositivo es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El tipo de dispositivo no puede superar los 50 caracteres.")]
    public string DeviceType { get; set; } = string.Empty;
}

public sealed class EnviarNotificacionUsuarioDto
{
    [Required(ErrorMessage = "El título de la notificación es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cuerpo del mensaje es obligatorio.")]
    [MaxLength(1000, ErrorMessage = "El mensaje no puede superar los 1000 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    public Dictionary<string, string>? DatosAdicionales { get; set; }
}
