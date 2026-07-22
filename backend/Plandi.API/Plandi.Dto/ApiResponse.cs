namespace Plandi.Dto;

/// <summary>
/// Respuesta estándar para todas las APIs del sistema.
/// El front siempre recibe { success, data, message } — consistente.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>Indica si la operación fue exitosa.</summary>
    public bool Success { get; set; }

    /// <summary>Los datos de respuesta (puede ser null en errores).</summary>
    public T? Data { get; set; }

    /// <summary>Mensaje informativo o de error.</summary>
    public string Message { get; set; } = string.Empty;
}
