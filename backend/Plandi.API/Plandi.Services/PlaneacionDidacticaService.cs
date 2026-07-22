using Microsoft.EntityFrameworkCore;
using Plandi.Dto;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;
using Plandi.Services.Mappers;

namespace Plandi.Services;

public class PlaneacionDidacticaService : IPlaneacionDidacticaService
{
    private readonly AppDbContext _dbContext;

    public PlaneacionDidacticaService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Obtiene una planeación por ID y la mapea a DTO de revisor.
    /// El Include carga las entidades necesarias para el mapeo anidado.
    /// </summary>
    public async Task<PlaneacionDidacticaRevisorDto?> GetByIdForRevisorAsync(long id)
    {
        var planeacion = await _dbContext.PlaneacionesDidacticas
            .AsNoTracking()
            .Include(p => p.Asignatura)
            .Include(p => p.Periodo)
            .Include(p => p.Academia)
            .FirstOrDefaultAsync(p => p.Id == id);

        // Mapperly genera el mapeo en compile-time: cero reflection, cero runtime cost
        return planeacion?.ToRevisorDto();
    }

    public async Task<List<PlaneacionDidacticaRevisorDto>> GetAllPlaneacionesForIdRevisor(int idRevisor)
    {
        var planeaciones = await _dbContext.PlaneacionesDidacticas
            .AsNoTracking()
            .Include(p => p.Asignatura)
            .Include(p => p.Periodo)
            .Include(p => p.Academia)
            .Where(p => p.RevisorId == idRevisor)
            .Select(p => p.ToRevisorDto())
            .ToListAsync();
        return planeaciones;
    }
    
    /// <summary>
    /// Obtiene todas las planeaciones y las mapea a DTOs de revisor.
    /// </summary>
    public async Task<List<PlaneacionDidacticaRevisorDto>> GetAllForRevisorAsync()
    {
        var planeaciones = await _dbContext.PlaneacionesDidacticas
            .AsNoTracking()
            .Include(p => p.Asignatura)
            .Include(p => p.Periodo)
            .Include(p => p.Academia)
            .ToListAsync();

        // El método de extensión ToRevisorDto() funciona en cualquier IEnumerable
        return planeaciones
            .Select(p => p.ToRevisorDto())
            .ToList();
    }

    /// <summary>
    /// Obtiene planeaciones agrupadas por carrera para un directivo.
    /// Flujo: directivo → AcademiaUsuario → academias → CarreraAcademia → carreras
    ///                               ↘ PlaneacionDidactica.AcademiaId → planeaciones
    /// </summary>
    public async Task<List<CarreraPlaneacionDto>> GetPlaneacionesByDirectivoAsync(int directivoId)
    {
        // 1. Academias del directivo
        var academiaIds = await _dbContext.AcademiaUsuarios
            .Where(au => au.UsuarioId == directivoId)
            .Select(au => au.AcademiaId)
            .ToListAsync();

        if (academiaIds.Count == 0)
            return new List<CarreraPlaneacionDto>();

        // 2. (CarreraId, AcademiaId) pairs vía la pivot CarreraAcademia
        var carreraAcademiaPairs = await _dbContext.CarreraAcademias
            .Where(ca => academiaIds.Contains(ca.AcademiaId))
            .Select(ca => new { ca.CarreraId, ca.AcademiaId })
            .ToListAsync();

        if (carreraAcademiaPairs.Count == 0)
            return new List<CarreraPlaneacionDto>();

        // 3. Planeaciones de esas academias (con Includes para el DTO directivo)
        var planeaciones = await _dbContext.PlaneacionesDidacticas
            .AsNoTracking()
            .Include(p => p.Asignatura)
            .Include(p => p.Periodo)
            .Include(p => p.Academia)
            .Include(p => p.Revisor)
            .Include(p => p.UltimaModificacionPor)
            .Include(p => p.PlaneacionDocentes)
                .ThenInclude(pd => pd.Docente)
            .Where(p => p.AcademiaId != null && academiaIds.Contains(p.AcademiaId.Value))
            .ToListAsync();

        // 4. Pre-cargar nombres de carreras (evita N+1)
        var carreraIds = carreraAcademiaPairs
            .Select(ca => ca.CarreraId)
            .Distinct();

        var nombresCarreras = await _dbContext.Carreras
            .Where(c => carreraIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Nombre);

        // 5. Build lookup: AcademiaId → CarreraIds
        var academiaToCarreras = carreraAcademiaPairs
            .GroupBy(ca => ca.AcademiaId)
            .ToDictionary(g => g.Key, g => g.Select(ca => ca.CarreraId).Distinct());

        // 6. Map planeaciones and group by carrera
        var resultado = new Dictionary<long, CarreraPlaneacionDto>();

        foreach (var planeacion in planeaciones)
        {
            if (planeacion.AcademiaId is null) continue;

            var dto = planeacion.ToDirectivoDto();

            // La planeacion pertenece a TODAS las carreras vinculadas a esa academia
            if (!academiaToCarreras.TryGetValue(planeacion.AcademiaId.Value, out var carrerasDeAcademia))
                continue;

            foreach (var cId in carrerasDeAcademia)
            {
                if (!resultado.TryGetValue(cId, out var grupo))
                {
                    grupo = new CarreraPlaneacionDto
                    {
                        NombreCarrera = nombresCarreras.GetValueOrDefault(cId, $"Carrera #{cId}"),
                        PlaneacionesCarrera = new List<PlaneacionDirectivoDto>()
                    };
                    resultado[cId] = grupo;
                }

                grupo.PlaneacionesCarrera.Add(dto);
            }
        }

        return resultado.Values.ToList();
    }

    /// <summary>
    /// Busca planeaciones con filtros opcionales aplicados EN SQL.
    /// Soportados: carrera, periodo, asignatura, docente, fecha (última modificación), estado.
    /// </summary>
    public async Task<List<PlaneacionDirectivoDto>> GetAllAsync(PlaneacionFilterDto filtro)
    {
        // ── Base query con Includes necesarios para ToDirectivoDto() ──
        var query = _dbContext.PlaneacionesDidacticas
            .AsNoTracking()
            .Include(p => p.Asignatura)
            .Include(p => p.Periodo)
            .Include(p => p.Academia)
            .Include(p => p.Revisor)
            .Include(p => p.UltimaModificacionPor)
            .Include(p => p.PlaneacionDocentes)
                .ThenInclude(pd => pd.Docente)
            .AsQueryable();

        // ── Filtro por Carrera (vía CarreraAcademia pivot) ──
        if (filtro.CarreraId.HasValue)
        {
            var academiaIds = await _dbContext.CarreraAcademias
                .Where(ca => ca.CarreraId == filtro.CarreraId.Value)
                .Select(ca => ca.AcademiaId)
                .ToListAsync();

            query = query.Where(p => p.AcademiaId != null
                && academiaIds.Contains(p.AcademiaId.Value));
        }

        // ── Filtro por Periodo ──
        if (filtro.PeriodoId.HasValue)
            query = query.Where(p => p.PeriodoId == filtro.PeriodoId.Value);

        // ── Filtro por Asignatura ──
        if (filtro.AsignaturaId.HasValue)
            query = query.Where(p => p.AsignaturaId == filtro.AsignaturaId.Value);

        // ── Filtro por Docente (vía PlaneacionDocentes) ──
        if (filtro.DocenteId.HasValue)
            query = query.Where(p => p.PlaneacionDocentes
                .Any(pd => pd.DocenteId == filtro.DocenteId.Value));

        // ── Filtro por fecha de última modificación ──
        if (filtro.FechaDesde.HasValue)
            query = query.Where(p => p.FechaUltimaModificacion >= filtro.FechaDesde.Value);

        if (filtro.FechaHasta.HasValue)
            query = query.Where(p => p.FechaUltimaModificacion <= filtro.FechaHasta.Value);

        // ── Filtro por Estado ──
        if (filtro.Estado.HasValue)
            query = query.Where(p => p.Estado == filtro.Estado.Value);

        // ── Ejecutar y mapear ──
        var planeaciones = await query.ToListAsync();
        return planeaciones.Select(p => p.ToDirectivoDto()).ToList();
    }

    /// <summary>
    /// Actualiza una planeación existente desde un DTO.
    /// Sobreescribe solo los campos del DTO, el resto se mantiene igual.
    /// </summary>
    public async Task UpdateFromDtoAsync(long id, PlaneacionDidacticaRevisorDto dto)
    {
        var planeacion = await _dbContext.PlaneacionesDidacticas
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new InvalidOperationException($"Planeación {id} no encontrada");

        // UpdateFromDto sobreescribe solo los campos del DTO en la entity
        planeacion.UpdateFromDto(dto);

        await _dbContext.SaveChangesAsync();
    }
}
