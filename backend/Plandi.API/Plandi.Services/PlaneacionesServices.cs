using AutoMapper;
using Plandi.Dto;
using Plandi.Dto.Enums;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plandi.Services
{
    public class PlaneacionCaratulaService : IPlaneacionCaratulaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PlaneacionCaratulaService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PlaneacionCaratulaDto?> GetByPlaneacionIdAsync(long planeacionId)
        {
            var caratula = await _context.PlaneacionCaratulas
                .FirstOrDefaultAsync(x => x.PlaneacionDidacticaId == planeacionId && x.Activo);

            return caratula == null ? null : _mapper.Map<PlaneacionCaratulaDto>(caratula);
        }

        public async Task<PlaneacionCaratulaDto> CreateAsync(long planeacionId, CreatePlaneacionCaratulaDto dto)
        {
            var planeacion = await _context.PlaneacionesDidacticas.FindAsync(planeacionId);
            if (planeacion == null)
                throw new InvalidOperationException($"Planeación con Id {planeacionId} no encontrada.");

            var caratula = _mapper.Map<PlaneacionCaratula>(dto);
            caratula.PlaneacionDidacticaId = planeacionId;

            _context.PlaneacionCaratulas.Add(caratula);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionCaratulaDto>(caratula);
        }

        public async Task<PlaneacionCaratulaDto> UpdateAsync(long id, UpdatePlaneacionCaratulaDto dto)
        {
            var caratula = await _context.PlaneacionCaratulas.FindAsync(id);
            if (caratula == null)
                throw new InvalidOperationException($"Carátula con Id {id} no encontrada.");

            _mapper.Map(dto, caratula);


            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionCaratulaDto>(caratula);
        }

        public async Task DeleteAsync(long id)
        {
            var caratula = await _context.PlaneacionCaratulas.FindAsync(id);
            if (caratula == null)
                throw new InvalidOperationException($"Carátula con Id {id} no encontrada.");

            caratula.Activo = false;
            caratula.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }

    public class PlaneacionTemaService : IPlaneacionTemaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PlaneacionTemaService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PlaneacionTemaDto> GetByIdAsync(long id)
        {
            var tema = await _context.PlaneacionTemas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.Activo);

            if (tema == null)
                throw new InvalidOperationException($"Tema con Id {id} no encontrado.");

            return _mapper.Map<PlaneacionTemaDto>(tema);
        }

        public async Task<List<PlaneacionTemaDto>> GetByUnidadIdAsync(long unidadId)
        {
            var temas = await _context.PlaneacionTemas
                .AsNoTracking()
                .Where(x => x.PlaneacionUnidadId == unidadId && x.Activo)
                .OrderBy(x => x.Orden)
                .ToListAsync();

            return _mapper.Map<List<PlaneacionTemaDto>>(temas);
        }

        public async Task<PlaneacionTemaDto> CreateAsync(long unidadId, CreatePlaneacionTemaDtos dto)
        {
            var unidad = await _context.PlaneacionUnidades.FindAsync(unidadId);
            if (unidad == null)
                throw new InvalidOperationException($"Unidad con Id {unidadId} no encontrada.");

            var tema = _mapper.Map<PlaneacionTema>(dto);
            tema.PlaneacionUnidadId = unidadId;

            _context.PlaneacionTemas.Add(tema);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionTemaDto>(tema);
        }

        public async Task<PlaneacionTemaDto> UpdateAsync(long id, UpdatePlaneacionTemaDtos dto)
        {
            var tema = await _context.PlaneacionTemas.FindAsync(id);
            if (tema == null)
                throw new InvalidOperationException($"Tema con Id {id} no encontrado.");

            _mapper.Map(dto, tema);
            tema.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionTemaDto>(tema);
        }

        public async Task DeleteAsync(long id)
        {
            var tema = await _context.PlaneacionTemas.FindAsync(id);
            if (tema == null)
                throw new InvalidOperationException($"Tema con Id {id} no encontrado.");

            tema.Activo = false;
            tema.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }

    public class PlaneacionEvaluacionService : IPlaneacionEvaluacionService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PlaneacionEvaluacionService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PlaneacionEvaluacionDto> GetByIdAsync(long id)
        {
            var evaluacion = await _context.PlaneacionEvaluaciones.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.Activo);

            if (evaluacion == null)
                throw new InvalidOperationException($"Evaluación con Id {id} no encontrada.");

            return _mapper.Map<PlaneacionEvaluacionDto>(evaluacion);
        }

        public async Task<List<PlaneacionEvaluacionDto>> GetByUnidadIdAsync(long unidadId)
        {
            var evaluaciones = await _context.PlaneacionEvaluaciones
                .AsNoTracking()
                .Where(x => x.PlaneacionUnidadId == unidadId && x.Activo)
                .OrderBy(x => x.Orden)
                .ToListAsync();

            return _mapper.Map<List<PlaneacionEvaluacionDto>>(evaluaciones);
        }

        public async Task<PlaneacionEvaluacionDto> CreateAsync(long unidadId, CreatePlaneacionEvaluacionDto dto)
        {
            var unidad = await _context.PlaneacionUnidades.FindAsync(unidadId);
            if (unidad == null)
                throw new InvalidOperationException($"Unidad con Id {unidadId} no encontrada.");

            var evaluacion = _mapper.Map<PlaneacionEvaluacion>(dto);
            evaluacion.PlaneacionUnidadId = unidadId;

            _context.PlaneacionEvaluaciones.Add(evaluacion);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionEvaluacionDto>(evaluacion);
        }

        public async Task<PlaneacionEvaluacionDto> UpdateAsync(long id, UpdatePlaneacionEvaluacionDto dto)
        {
            var evaluacion = await _context.PlaneacionEvaluaciones.FindAsync(id);
            if (evaluacion == null)
                throw new InvalidOperationException($"Evaluación con Id {id} no encontrada.");

            _mapper.Map(dto, evaluacion);
            evaluacion.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionEvaluacionDto>(evaluacion);
        }

        public async Task DeleteAsync(long id)
        {
            var evaluacion = await _context.PlaneacionEvaluaciones.FindAsync(id);
            if (evaluacion == null)
                throw new InvalidOperationException($"Evaluación con Id {id} no encontrada.");

            evaluacion.Activo = false;
            evaluacion.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }

    public class PlaneacionSecuenciaService : IPlaneacionSecuenciaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PlaneacionSecuenciaService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PlaneacionSecuenciaDto> GetByIdAsync(long id)
        {
            var secuencia = await _context.PlaneacionSecuencias.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.Activo);

            if (secuencia == null)
                throw new InvalidOperationException($"Secuencia con Id {id} no encontrada.");

            return _mapper.Map<PlaneacionSecuenciaDto>(secuencia);
        }

        public async Task<List<PlaneacionSecuenciaDto>> GetByUnidadIdAsync(long unidadId)
        {
            var secuencias = await _context.PlaneacionSecuencias
                .AsNoTracking()
                .Where(x => x.PlaneacionUnidadId == unidadId && x.Activo)
                .OrderBy(x => x.Orden)
                .ToListAsync();

            return _mapper.Map<List<PlaneacionSecuenciaDto>>(secuencias);
        }

        public async Task<PlaneacionSecuenciaDto> CreateAsync(long unidadId, CreatePlaneacionSecuenciaDto dto)
        {
            var unidad = await _context.PlaneacionUnidades.FindAsync(unidadId);
            if (unidad == null)
                throw new InvalidOperationException($"Unidad con Id {unidadId} no encontrada.");

            // Validar que la estrategia sea válida para la fase seleccionada
            if (!EConverter.IsValidStrategyForPhase(dto.Fase, dto.Estrategia))
                throw new InvalidOperationException(
                    $"La estrategia con valor {dto.Estrategia} no es válida para la fase {dto.Fase}. " +
                    $"Seleccione una estrategia correspondiente a la fase elegida.");

            var secuencia = _mapper.Map<PlaneacionSecuencia>(dto);
            secuencia.PlaneacionUnidadId = unidadId;

            _context.PlaneacionSecuencias.Add(secuencia);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionSecuenciaDto>(secuencia);
        }

        public async Task<PlaneacionSecuenciaDto> UpdateAsync(long id, UpdatePlaneacionSecuenciaDto dto)
        {
            var secuencia = await _context.PlaneacionSecuencias.FindAsync(id);
            if (secuencia == null)
                throw new InvalidOperationException($"Secuencia con Id {id} no encontrada.");

            // Si se está actualizando la fase y/o estrategia, validar que sean compatibles
            if (dto.Fase.HasValue && dto.Estrategia.HasValue)
            {
                if (!EConverter.IsValidStrategyForPhase(dto.Fase.Value, dto.Estrategia.Value))
                    throw new InvalidOperationException(
                        $"La estrategia con valor {dto.Estrategia} no es válida para la fase {dto.Fase}. " +
                        $"Seleccione una estrategia correspondiente a la fase elegida.");
            }
            else if (dto.Fase.HasValue && !dto.Estrategia.HasValue)
            {
                // Si solo se actualiza la fase, validar que la estrategia actual sea válida para la nueva fase
                if (!EConverter.IsValidStrategyForPhase(dto.Fase.Value, secuencia.Estrategia))
                    throw new InvalidOperationException(
                        $"La estrategia actual (valor {secuencia.Estrategia}) no es válida para la nueva fase {dto.Fase}. " +
                        $"Actualice también la estrategia a una correspondiente a la nueva fase elegida.");
            }
            else if (!dto.Fase.HasValue && dto.Estrategia.HasValue)
            {
                // Si solo se actualiza la estrategia, validar que sea válida para la fase actual
                if (!EConverter.IsValidStrategyForPhase(secuencia.Fase, dto.Estrategia.Value))
                    throw new InvalidOperationException(
                        $"La estrategia con valor {dto.Estrategia} no es válida para la fase actual {secuencia.Fase}. " +
                        $"Seleccione una estrategia correspondiente a la fase actual.");
            }

            _mapper.Map(dto, secuencia);
            secuencia.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionSecuenciaDto>(secuencia);
        }

        public async Task DeleteAsync(long id)
        {
            var secuencia = await _context.PlaneacionSecuencias.FindAsync(id);
            if (secuencia == null)
                throw new InvalidOperationException($"Secuencia con Id {id} no encontrada.");

            secuencia.Activo = false;
            secuencia.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }

    public class PlaneacionReferenciaService : IPlaneacionReferenciaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PlaneacionReferenciaService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PlaneacionReferenciaDto> GetByIdAsync(long id)
        {
            var referencia = await _context.PlaneacionReferencias.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.Activo);

            if (referencia == null)
                throw new InvalidOperationException($"Referencia con Id {id} no encontrada.");

            return _mapper.Map<PlaneacionReferenciaDto>(referencia);
        }

        public async Task<List<PlaneacionReferenciaDto>> GetByPlaneacionIdAsync(long planeacionId)
        {
            var referencias = await _context.PlaneacionReferencias
                .AsNoTracking()
                .Where(x => x.PlaneacionDidacticaId == planeacionId && x.Activo)
                .OrderBy(x => x.Orden)
                .ToListAsync();

            return _mapper.Map<List<PlaneacionReferenciaDto>>(referencias);
        }

        public async Task<PlaneacionReferenciaDto> CreateAsync(long planeacionId, CreatePlaneacionReferenciaDto dto)
        {
            var planeacion = await _context.PlaneacionesDidacticas.FindAsync(planeacionId);
            if (planeacion == null)
                throw new InvalidOperationException($"Planeación con Id {planeacionId} no encontrada.");

            var referencia = _mapper.Map<PlaneacionReferencia>(dto);
            referencia.PlaneacionDidacticaId = planeacionId;

            _context.PlaneacionReferencias.Add(referencia);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionReferenciaDto>(referencia);
        }

        public async Task<PlaneacionReferenciaDto> UpdateAsync(long id, UpdatePlaneacionReferenciaDto dto)
        {
            var referencia = await _context.PlaneacionReferencias.FindAsync(id);
            if (referencia == null)
                throw new InvalidOperationException($"Referencia con Id {id} no encontrada.");

            _mapper.Map(dto, referencia);
            referencia.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionReferenciaDto>(referencia);
        }

        public async Task DeleteAsync(long id)
        {
            var referencia = await _context.PlaneacionReferencias.FindAsync(id);
            if (referencia == null)
                throw new InvalidOperationException($"Referencia con Id {id} no encontrada.");

            referencia.Activo = false;
            referencia.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
