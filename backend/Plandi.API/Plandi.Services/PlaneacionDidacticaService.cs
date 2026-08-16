using Microsoft.EntityFrameworkCore;
using Plandi.Dto;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;
using Plandi.Services.Mappers;
using System.Data;

namespace Plandi.Services;

public class PlaneacionDidacticaService : IPlaneacionDidacticaService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificacionService _notificacionService;

    public PlaneacionDidacticaService(AppDbContext dbContext, INotificacionService notificacionService)
    {
        _dbContext = dbContext;
        _notificacionService = notificacionService;
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

    public async Task<(PlaneacionDetalleRevisionDto? Detalle, bool Exists, bool Authorized)> GetDetalleRevisionAsync(long id, long usuarioId)
    {
        var planeacionAuth = await _dbContext.PlaneacionesDidacticas
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.RevisorId,
                p.AcademiaId
            })
            .FirstOrDefaultAsync();

        if (planeacionAuth is null)
        {
            return (null, false, false);
        }

        var esRevisorAsignado = planeacionAuth.RevisorId == usuarioId;
        var esDirectivoOCoordinadorRelacionado = planeacionAuth.AcademiaId.HasValue
            && await IsUsuarioDirectivoRelacionadoAsync(usuarioId, planeacionAuth.AcademiaId.Value, incluirCoordinador: true);

        if (!esRevisorAsignado && !esDirectivoOCoordinadorRelacionado)
        {
            return (null, true, false);
        }

        var planeacion = await _dbContext.PlaneacionesDidacticas
            .AsNoTracking()
            .Include(p => p.Asignatura)
            .Include(p => p.Periodo)
            .Include(p => p.Academia)
            .Include(p => p.UltimaModificacionPor)
            .FirstAsync(p => p.Id == id);

        var unidades = await _dbContext.PlaneacionUnidades
            .AsNoTracking()
            .Where(u => u.PlaneacionDidacticaId == id)
            .OrderBy(u => u.Orden)
            .ThenBy(u => u.Id)
            .Select(u => new
            {
                u.Id,
                u.Numero,
                u.Nombre
            })
            .ToListAsync();

        var unidadIds = unidades.Select(u => u.Id).ToList();

        var observaciones = await _dbContext.PlaneacionObservaciones
            .AsNoTracking()
            .Include(o => o.Revisor)
            .Where(o => o.PlaneacionDidacticaId == id
                && (!o.PlaneacionUnidadId.HasValue
                    || unidadIds.Contains(o.PlaneacionUnidadId.Value)))
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Select(o => new
            {
                o.Id,
                UnidadId = o.PlaneacionUnidadId,
                o.Comentario,
                o.CreatedAt,
                AutorNombre = o.Revisor.Nombre,
                AutorApellidoPaterno = o.Revisor.ApellidoPaterno,
                AutorApellidoMaterno = o.Revisor.ApellidoMaterno
            })
            .ToListAsync();

        var actividades = await _dbContext.PlaneacionActividades
            .AsNoTracking()
            .Where(a => unidadIds.Contains(a.PlaneacionUnidadId))
            .OrderBy(a => a.Orden)
            .ThenBy(a => a.Id)
            .Select(a => new
            {
                a.Id,
                a.PlaneacionUnidadId,
                a.Descripcion,
                a.Semana,
                a.Horas,
                a.EstrategiaEnsenanza,
                a.Evidencia,
                a.InstrumentoEvaluacion,
                a.PorcentajeEvaluacion
            })
            .ToListAsync();

        var observacionesPorUnidad = observaciones
            .Where(o => o.UnidadId.HasValue)
            .GroupBy(o => o.UnidadId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(o => new PlaneacionDetalleRevisionObservacionDto
                {
                    Id = o.Id,
                    Comentario = o.Comentario,
                    Autor = BuildNombreUsuario(o.AutorNombre, o.AutorApellidoPaterno, o.AutorApellidoMaterno),
                    Fecha = o.CreatedAt.ToString("yyyy-MM-dd")
                }).ToList());

        var observacionesPlaneacion = observaciones
            .Where(o => !o.UnidadId.HasValue)
            .Select(o => new PlaneacionDetalleRevisionObservacionDto
            {
                Id = o.Id,
                Comentario = o.Comentario,
                Autor = BuildNombreUsuario(o.AutorNombre, o.AutorApellidoPaterno, o.AutorApellidoMaterno),
                Fecha = o.CreatedAt.ToString("yyyy-MM-dd")
            })
            .ToList();

        var actividadesPorUnidad = actividades
            .GroupBy(a => a.PlaneacionUnidadId)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(a => a.Semana)
                    .OrderBy(semana => semana.Key.HasValue ? 1 : 0)
                    .ThenBy(semana => semana.Key)
                    .Select(semana => new PlaneacionDetalleRevisionActividadesPorSemanaDto
                    {
                        Semana = semana.Key,
                        Titulo = semana.Key.HasValue ? $"Semana {semana.Key.Value}" : "Sin semana",
                        Actividades = semana.Select(a => new PlaneacionDetalleRevisionActividadDto
                        {
                            Id = a.Id,
                            Descripcion = a.Descripcion,
                            Semana = a.Semana,
                            Horas = a.Horas,
                            Estrategia = a.EstrategiaEnsenanza ?? string.Empty,
                            Evidencia = a.Evidencia ?? string.Empty,
                            Instrumento = a.InstrumentoEvaluacion ?? string.Empty,
                            PorcentajeEvaluacion = a.PorcentajeEvaluacion
                        }).ToList()
                    }).ToList());

        var detalle = new PlaneacionDetalleRevisionDto
        {
            Id = planeacion.Id,
            Caratula = new PlaneacionDetalleRevisionCaratulaDto
            {
                Titulo = planeacion.Titulo,
                Asignatura = planeacion.Asignatura?.Nombre ?? string.Empty,
                Periodo = planeacion.Periodo?.Nombre ?? string.Empty,
                Academia = planeacion.Academia?.Nombre ?? string.Empty,
                Estado = planeacion.Estado,
                UltimaModificacion = new PlaneacionDetalleRevisionUltimaModificacionDto
                {
                    Fecha = planeacion.FechaUltimaModificacion?.ToString("yyyy-MM-dd") ?? string.Empty,
                    Usuario = planeacion.UltimaModificacionPor is null
                        ? string.Empty
                        : BuildNombreUsuario(
                            planeacion.UltimaModificacionPor.Nombre,
                            planeacion.UltimaModificacionPor.ApellidoPaterno,
                            planeacion.UltimaModificacionPor.ApellidoMaterno)
                }
            },
            Observaciones = observacionesPlaneacion,
            Unidades = unidades.Select(u => new PlaneacionDetalleRevisionUnidadDto
            {
                Id = u.Id,
                Numero = u.Numero,
                Nombre = u.Nombre,
                Observaciones = observacionesPorUnidad.GetValueOrDefault(u.Id, new List<PlaneacionDetalleRevisionObservacionDto>()),
                ActividadesPorSemana = actividadesPorUnidad.GetValueOrDefault(u.Id, new List<PlaneacionDetalleRevisionActividadesPorSemanaDto>())
            }).ToList()
        };

        return (detalle, true, true);
    }

    public async Task<(PlaneacionObservacionDto? Observacion, bool Exists, bool Authorized, bool UnidadValid)> CrearObservacionAsync(
        long id,
        long usuarioId,
        CrearPlaneacionObservacionRequestDto request)
    {
        var planeacion = await _dbContext.PlaneacionesDidacticas
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.RevisorId
            })
            .FirstOrDefaultAsync();

        if (planeacion is null)
        {
            return (null, false, false, false);
        }

        if (planeacion.RevisorId != usuarioId)
        {
            return (null, true, false, false);
        }

        if (request.UnidadId.HasValue)
        {
            var unidadPerteneceAPlaneacion = await _dbContext.PlaneacionUnidades
                .AsNoTracking()
                .AnyAsync(u => u.Id == request.UnidadId.Value
                    && u.PlaneacionDidacticaId == id);

            if (!unidadPerteneceAPlaneacion)
            {
                return (null, true, true, false);
            }
        }

        var comentario = request.Comentario.Trim();
        var now = DateTime.UtcNow;

        var observacion = new PlaneacionObservacion
        {
            PlaneacionDidacticaId = id,
            PlaneacionUnidadId = request.UnidadId,
            RevisorId = usuarioId,
            Comentario = comentario,
            Estado = "ABIERTA",
            CreatedAt = now
        };

        _dbContext.PlaneacionObservaciones.Add(observacion);
        await _dbContext.SaveChangesAsync();

        var revisor = await _dbContext.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == usuarioId)
            .Select(u => new
            {
                u.Nombre,
                u.ApellidoPaterno,
                u.ApellidoMaterno
            })
            .FirstOrDefaultAsync();

        return (new PlaneacionObservacionDto
        {
            Id = observacion.Id,
            PlaneacionDidacticaId = observacion.PlaneacionDidacticaId,
            UnidadId = observacion.PlaneacionUnidadId,
            RevisorId = observacion.RevisorId,
            Comentario = observacion.Comentario,
            Estado = observacion.Estado,
            Autor = revisor is null
                ? string.Empty
                : BuildNombreUsuario(revisor.Nombre, revisor.ApellidoPaterno, revisor.ApellidoMaterno),
            Fecha = observacion.CreatedAt.ToString("yyyy-MM-dd")
        }, true, true, true);
    }

    public async Task<(PlaneacionEstadoDto? Planeacion, bool Exists, bool Authorized)> AutorizarAsync(long id, long usuarioId)
    {
        var planeacion = await _dbContext.PlaneacionesDidacticas
            .FirstOrDefaultAsync(p => p.Id == id);

        if (planeacion is null)
        {
            return (null, false, false);
        }

        var esRevisorAsignado = planeacion.RevisorId == usuarioId;
        var esDirectivoRelacionado = planeacion.AcademiaId.HasValue
            && await IsUsuarioDirectivoRelacionadoAsync(usuarioId, planeacion.AcademiaId.Value, incluirCoordinador: false);

        if (!esRevisorAsignado && !esDirectivoRelacionado)
        {
            return (null, true, false);
        }

        var notificarDocentes = planeacion.Estado != EstadoPlaneacion.Aprobada;

        if (notificarDocentes)
        {
            var now = DateTime.UtcNow;
            planeacion.Estado = EstadoPlaneacion.Aprobada;
            planeacion.FechaUltimaModificacion = now;
            planeacion.UltimaModificacionPorId = usuarioId;
            planeacion.UpdatedAt = now;

            await _dbContext.SaveChangesAsync();
            await _notificacionService.NotificarPlaneacionAutorizadaAsync(planeacion.Id);
        }

        return (new PlaneacionEstadoDto
        {
            Id = planeacion.Id,
            Estado = planeacion.Estado,
            FechaUltimaModificacion = planeacion.FechaUltimaModificacion?.ToString("yyyy-MM-dd") ?? string.Empty,
            UsuarioUltimaModificacion = planeacion.UltimaModificacionPorId
        }, true, true);
    }

    public async Task<(PlaneacionRevisionSolicitadaDto? Planeacion, bool Exists, bool Authorized, bool HasRevisor, bool HasDocentes)> SolicitarRevisionAsync(long id, long usuarioId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var planeacion = await _dbContext.PlaneacionesDidacticas
            .Include(p => p.PlaneacionDocentes)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (planeacion is null)
        {
            return (null, false, false, false, false);
        }

        var docenteIds = planeacion.PlaneacionDocentes
            .Where(pd => pd.Activo)
            .Select(pd => pd.DocenteId)
            .Distinct()
            .ToList();

        if (docenteIds.Count == 0)
        {
            return (null, true, false, planeacion.RevisorId.HasValue, false);
        }

        if (!docenteIds.Contains(usuarioId))
        {
            return (null, true, false, planeacion.RevisorId.HasValue, true);
        }

        if (!planeacion.RevisorId.HasValue)
        {
            return (null, true, true, false, true);
        }

        var now = DateTime.UtcNow;
        var revisorId = planeacion.RevisorId.Value;

        planeacion.Estado = EstadoPlaneacion.EnRevision;
        planeacion.FechaUltimaModificacion = now;
        planeacion.UltimaModificacionPorId = usuarioId;
        planeacion.UpdatedAt = now;

        var chat = await _dbContext.Chats
            .FirstOrDefaultAsync(c => c.PlaneacionDidacticaId == id);

        if (chat is null)
        {
            chat = new Chat
            {
                PlaneacionDidacticaId = id,
                Titulo = BuildChatTitulo(planeacion.Titulo),
                CreatedAt = now
            };

            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();
        }

        var participantesDeseados = docenteIds
            .ToDictionary(docenteId => docenteId, _ => "DOCENTE");
        participantesDeseados[revisorId] = "REVISOR";

        var participantesExistentes = await _dbContext.ChatParticipantes
            .Where(cp => cp.ChatId == chat.Id)
            .ToDictionaryAsync(cp => cp.UsuarioId);

        foreach (var participante in participantesDeseados)
        {
            if (participantesExistentes.TryGetValue(participante.Key, out var existente))
            {
                existente.Activo = true;
                if (participante.Value == "REVISOR")
                {
                    existente.RolEnChat = "REVISOR";
                }

                continue;
            }

            _dbContext.ChatParticipantes.Add(new ChatParticipante
            {
                ChatId = chat.Id,
                UsuarioId = participante.Key,
                RolEnChat = participante.Value,
                Activo = true,
                CreatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        var participantesActivos = await _dbContext.ChatParticipantes
            .AsNoTracking()
            .CountAsync(cp => cp.ChatId == chat.Id && cp.Activo);

        return (new PlaneacionRevisionSolicitadaDto
        {
            Id = planeacion.Id,
            Estado = planeacion.Estado,
            ChatId = chat.Id,
            Participantes = participantesActivos,
            FechaUltimaModificacion = planeacion.FechaUltimaModificacion?.ToString("yyyy-MM-dd") ?? string.Empty,
            UsuarioUltimaModificacion = planeacion.UltimaModificacionPorId
        }, true, true, true, true);
    }

    public async Task<(PlaneacionRechazoDto? Planeacion, bool Exists, bool Authorized)> RechazarAsync(
        long id,
        long usuarioId,
        PlaneacionRechazarRequestDto request)
    {
        var planeacion = await _dbContext.PlaneacionesDidacticas
            .FirstOrDefaultAsync(p => p.Id == id);

        if (planeacion is null)
        {
            return (null, false, false);
        }

        if (planeacion.RevisorId != usuarioId)
        {
            return (null, true, false);
        }

        var motivo = request.Motivo.Trim();
        var now = DateTime.UtcNow;
        var notificarDocentes = planeacion.Estado != EstadoPlaneacion.EnProceso;

        var observacion = new PlaneacionObservacion
        {
            PlaneacionDidacticaId = id,
            RevisorId = usuarioId,
            Comentario = motivo,
            Estado = "ABIERTA",
            CreatedAt = now
        };

        planeacion.Estado = EstadoPlaneacion.EnProceso;
        planeacion.FechaUltimaModificacion = now;
        planeacion.UltimaModificacionPorId = usuarioId;
        planeacion.UpdatedAt = now;

        _dbContext.PlaneacionObservaciones.Add(observacion);
        await _dbContext.SaveChangesAsync();

        if (notificarDocentes)
        {
            await _notificacionService.NotificarPlaneacionRechazadaAsync(planeacion.Id, motivo);
        }

        return (new PlaneacionRechazoDto
        {
            Id = planeacion.Id,
            Estado = planeacion.Estado,
            FechaUltimaModificacion = planeacion.FechaUltimaModificacion?.ToString("yyyy-MM-dd") ?? string.Empty,
            UsuarioUltimaModificacion = planeacion.UltimaModificacionPorId,
            Motivo = observacion.Comentario,
            ObservacionId = observacion.Id
        }, true, true);
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

    private static string BuildNombreUsuario(string nombre, string apellidoPaterno, string? apellidoMaterno)
        => string.Join(' ', new[] { nombre, apellidoPaterno, apellidoMaterno }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string BuildChatTitulo(string tituloPlaneacion)
    {
        var titulo = $"Chat - {tituloPlaneacion}";
        return titulo.Length <= 250 ? titulo : titulo[..250];
    }

    private async Task<bool> IsUsuarioDirectivoRelacionadoAsync(long usuarioId, long academiaId, bool incluirCoordinador)
    {
        var esDirector = await _dbContext.UsuarioRoles
            .AsNoTracking()
            .AnyAsync(ur => ur.UsuarioId == usuarioId && ur.Rol.Nombre == "Director");

        return await _dbContext.AcademiaUsuarios
            .AsNoTracking()
            .AnyAsync(au => au.Activo
                && au.AcademiaId == academiaId
                && au.UsuarioId == usuarioId
                && (esDirector
                    || au.RolEnAcademia == RolAcademia.JefeAcademia
                    || (incluirCoordinador && au.RolEnAcademia == RolAcademia.Coordinador)));
    }
}
