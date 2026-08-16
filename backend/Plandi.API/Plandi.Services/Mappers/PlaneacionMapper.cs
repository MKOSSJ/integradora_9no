using Plandi.Dto;
using Plandi.Library.Models;
using Riok.Mapperly.Abstractions;

namespace Plandi.Services.Mappers;

/// <summary>
/// Mapper para PlaneacionDidactica.
/// Mapperly genera el código de mapeo en compile-time (source generator).
/// Las propiedades con el mismo nombre y tipo se mapean automáticamente.
/// </summary>
[Mapper]
public static partial class PlaneacionMapper
{
    // ─── Conversiones manuales ─────────────────────────────────────

    /// <summary>
    /// Convierte long? → int para UsuarioUltimaModificacion.
    /// Mapperly detecta esta convención por el nombre: Map[SourceType]To[TargetType].
    /// </summary>
    [UserMapping]
    private static int MapLongToInt(long? value) => (int)(value ?? 0);

    /// <summary>
    /// Convierte int → long? para UltimaModificacionPorId.
    /// </summary>
    [UserMapping]
    private static long? MapIntToLong(int value) => value == 0 ? null : (long?)value;

    // ─── Entity → DTO ──────────────────────────────────────────────

    /// <summary>
    /// Mapea PlaneacionDidactica → PlaneacionDidacticaRevisorDto.
    /// Propiedades anidadas: Asignatura.Nombre → AsignaturaNombre, etc.
    /// Propiedades con mismo nombre y tipo: Titulo, Estado, FechaUltimaModificacion.
    /// Conversión de tipo: UltimaModificacionPorId (long?) → UsuarioUltimaModificacion (int)
    /// </summary>
    [MapProperty("Asignatura.Nombre", "AsignaturaNombre")]
    [MapProperty("Periodo.Nombre", "PeriodoNombre")]
    [MapProperty("Periodo.FechaInicio", "PeriodoFechaInicio")]
    [MapProperty("Periodo.FechaFin", "PeriodoFechaFin")]
    [MapProperty("Academia.Nombre", "AcademiaNombre")]
    public static partial PlaneacionDidacticaRevisorDto ToRevisorDto(this PlaneacionDidactica planeacion);

    private static string MapDateTimeToString(DateTime fecha)
        => fecha.ToString("yyyy-MM-dd");
    private static string MapNullableDateTimeToString(DateTime? fecha)
    => fecha?.ToString("yyyy-MM-dd");
    
    // NOTA: Mapperly genera automáticamente ToRevisorDto(ICollection<...>)
    // cuando definís ToRevisorDto(PlaneacionDidactica).
    // No necesitás declararlo - usalo así:
    //   var dtos = planeaciones.ToRevisorDto().ToList();

    // ─── DTO → Entity (update parcial) ─────────────────────────────
    
    /// <summary>
    /// Mapea campos del DTO de vuelta a la entity existente.
    /// Útil para updates: recibes la entity de BD y le sobreescribes
    /// solo los campos que el DTO trae.
    /// </summary>
    public static partial void UpdateFromDto(this PlaneacionDidactica planeacion, PlaneacionDidacticaRevisorDto dto);

    // ─── Directivo DTO (manual — Mapperly no soporta colecciones anidadas + nombres concatenados) ──

    /// <summary>
    /// Mapea PlaneacionDidactica → PlaneacionDirectivoDto.
    /// Reusa los Include necesarios: Asignatura, Periodo, Academia, Revisor,
    /// UltimaModificacionPor, PlaneacionDocentes[0].Docente.
    /// </summary>
    public static PlaneacionDirectivoDto ToDirectivoDto(this PlaneacionDidactica p)
    {
        var revisor = p.Revisor;
        var docente = p.PlaneacionDocentes?.FirstOrDefault()?.Docente;
        var ultMod = p.UltimaModificacionPor;

        return new PlaneacionDirectivoDto
        {
            // ── Heredados de PlaneacionDidacticaRevisorDto ──
            AsignaturaNombre         = p.Asignatura?.Nombre ?? string.Empty,
            PeriodoNombre            = p.Periodo?.Nombre ?? string.Empty,
            PeriodoFechaInicio       = p.Periodo?.FechaInicio.ToString("yyyy-MM-dd") ?? string.Empty,
            PeriodoFechaFin          = p.Periodo?.FechaFin.ToString("yyyy-MM-dd") ?? string.Empty,
            AcademiaNombre           = p.Academia?.Nombre ?? string.Empty,
            ProgramaAsignaturaId     = (int)(p.ProgramaAsignaturaId ?? 0),
            Titulo                   = p.Titulo,
            Estado                   = p.Estado,
            FechaUltimaModificacion  = p.FechaUltimaModificacion?.ToString("yyyy-MM-dd") ?? string.Empty,
            UsuarioUltimaModificacion = (int)(p.UltimaModificacionPorId ?? 0),

            // ── Propios de PlaneacionDirectivoDto ──
            NombreRevisor       = revisor is not null ? $"{revisor.Nombre} {revisor.ApellidoPaterno}" : string.Empty,
            IdRevisor           = (int)(p.RevisorId ?? 0),
            NombreMaestro       = docente is not null ? $"{docente.Nombre} {docente.ApellidoPaterno}" : string.Empty,
            IdMaestro           = docente is not null ? (int)docente.Id : 0,
            NombreUltimoModificacion = ultMod is not null ? $"{ultMod.Nombre} {ultMod.ApellidoPaterno}" : string.Empty,
            IdUltimoModificacion = (int)(p.UltimaModificacionPorId ?? 0),
        };
    }
}
