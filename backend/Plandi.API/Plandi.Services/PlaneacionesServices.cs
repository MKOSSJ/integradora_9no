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

        public async Task<PlaneacionCaratulaDto?> GetByPlaneacionIdAsync(Guid planeacionPublicId)
        {
            var caratula = await _context.PlaneacionCaratulas
                .Include(x => x.PlaneacionDidactica)
                .Include(x => x.ProgramaAsignatura)
                .FirstOrDefaultAsync(x => x.PlaneacionDidactica.PublicId == planeacionPublicId && x.Activo);

            return caratula == null ? null : _mapper.Map<PlaneacionCaratulaDto>(caratula);
        }

        public async Task<PlaneacionCaratulaDto> CreateAsync(Guid planeacionPublicId, CreatePlaneacionCaratulaDto dto)
        {
            var planeacion = await PlaneacionLegacySupport.BuscarPlaneacionAsync(_context, planeacionPublicId);
            PlaneacionLegacySupport.ExigirMutable(planeacion);

            var caratula = _mapper.Map<PlaneacionCaratula>(dto);
            caratula.PlaneacionDidactica = planeacion;
            if (dto.ProgramaAsignaturaPublicId.HasValue)
                caratula.ProgramaAsignatura = await PlaneacionLegacySupport.BuscarProgramaAsync(_context, dto.ProgramaAsignaturaPublicId.Value);

            _context.PlaneacionCaratulas.Add(caratula);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionCaratulaDto>(caratula);
        }

        public async Task<PlaneacionCaratulaDto> UpdateAsync(Guid publicId, UpdatePlaneacionCaratulaDto dto)
        {
            var caratula = await _context.PlaneacionCaratulas
                .Include(x => x.PlaneacionDidactica)
                .Include(x => x.ProgramaAsignatura)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);
            if (caratula == null)
                throw new InvalidOperationException("Carátula no encontrada.");
            PlaneacionLegacySupport.ExigirMutable(caratula.PlaneacionDidactica);

            _mapper.Map(dto, caratula);
            if (dto.ProgramaAsignaturaPublicId.HasValue)
                caratula.ProgramaAsignatura = await PlaneacionLegacySupport.BuscarProgramaAsync(_context, dto.ProgramaAsignaturaPublicId.Value);


            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionCaratulaDto>(caratula);
        }

        public async Task DeleteAsync(Guid publicId)
        {
            var caratula = await _context.PlaneacionCaratulas.Include(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);
            if (caratula == null)
                throw new InvalidOperationException("Carátula no encontrada.");
            PlaneacionLegacySupport.ExigirMutable(caratula.PlaneacionDidactica);

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

        public async Task<PlaneacionTemaDto> GetByIdAsync(Guid publicId)
        {
            var tema = await _context.PlaneacionTemas.AsNoTracking()
                .Include(x => x.PlaneacionUnidad)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);

            if (tema == null)
                throw new InvalidOperationException("Tema no encontrado.");

            return _mapper.Map<PlaneacionTemaDto>(tema);
        }

        public async Task<List<PlaneacionTemaDto>> GetByUnidadIdAsync(Guid unidadPublicId)
        {
            var temas = await _context.PlaneacionTemas
                .AsNoTracking()
                .Include(x => x.PlaneacionUnidad)
                .Where(x => x.PlaneacionUnidad.PublicId == unidadPublicId && x.Activo)
                .OrderBy(x => x.Orden)
                .ToListAsync();

            return _mapper.Map<List<PlaneacionTemaDto>>(temas);
        }

        public async Task<PlaneacionTemaDto> CreateAsync(Guid unidadPublicId, CreatePlaneacionTemaDtos dto)
        {
            var unidad = await PlaneacionLegacySupport.BuscarUnidadAsync(_context, unidadPublicId);
            PlaneacionLegacySupport.ExigirMutable(unidad.PlaneacionDidactica);

            var tema = _mapper.Map<PlaneacionTema>(dto);
            tema.PlaneacionUnidad = unidad;

            _context.PlaneacionTemas.Add(tema);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionTemaDto>(tema);
        }

        public async Task<PlaneacionTemaDto> UpdateAsync(Guid publicId, UpdatePlaneacionTemaDtos dto)
        {
            var tema = await _context.PlaneacionTemas.Include(x => x.PlaneacionUnidad).ThenInclude(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);
            if (tema == null)
                throw new InvalidOperationException("Tema no encontrado.");
            PlaneacionLegacySupport.ExigirMutable(tema.PlaneacionUnidad.PlaneacionDidactica);

            _mapper.Map(dto, tema);
            tema.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionTemaDto>(tema);
        }

        public async Task DeleteAsync(Guid publicId)
        {
            var tema = await _context.PlaneacionTemas.Include(x => x.PlaneacionUnidad).ThenInclude(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);
            if (tema == null)
                throw new InvalidOperationException("Tema no encontrado.");
            PlaneacionLegacySupport.ExigirMutable(tema.PlaneacionUnidad.PlaneacionDidactica);

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

        public async Task<PlaneacionEvaluacionDto> GetByIdAsync(Guid publicId)
        {
            var evaluacion = await _context.PlaneacionEvaluaciones.AsNoTracking()
                .Include(x => x.PlaneacionUnidad)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);

            if (evaluacion == null)
                throw new InvalidOperationException("Evaluación no encontrada.");

            return _mapper.Map<PlaneacionEvaluacionDto>(evaluacion);
        }

        public async Task<List<PlaneacionEvaluacionDto>> GetByUnidadIdAsync(Guid unidadPublicId)
        {
            var evaluaciones = await _context.PlaneacionEvaluaciones
                .AsNoTracking()
                .Include(x => x.PlaneacionUnidad)
                .Where(x => x.PlaneacionUnidad.PublicId == unidadPublicId && x.Activo)
                .OrderBy(x => x.Orden)
                .ToListAsync();

            return _mapper.Map<List<PlaneacionEvaluacionDto>>(evaluaciones);
        }

        public async Task<PlaneacionEvaluacionDto> CreateAsync(Guid unidadPublicId, CreatePlaneacionEvaluacionDto dto)
        {
            var unidad = await PlaneacionLegacySupport.BuscarUnidadAsync(_context, unidadPublicId);
            PlaneacionLegacySupport.ExigirMutable(unidad.PlaneacionDidactica);

            var evaluacion = _mapper.Map<PlaneacionEvaluacion>(dto);
            evaluacion.PlaneacionUnidad = unidad;

            _context.PlaneacionEvaluaciones.Add(evaluacion);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionEvaluacionDto>(evaluacion);
        }

        public async Task<PlaneacionEvaluacionDto> UpdateAsync(Guid publicId, UpdatePlaneacionEvaluacionDto dto)
        {
            var evaluacion = await _context.PlaneacionEvaluaciones.Include(x => x.PlaneacionUnidad).ThenInclude(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);
            if (evaluacion == null)
                throw new InvalidOperationException("Evaluación no encontrada.");
            PlaneacionLegacySupport.ExigirMutable(evaluacion.PlaneacionUnidad.PlaneacionDidactica);

            _mapper.Map(dto, evaluacion);
            evaluacion.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionEvaluacionDto>(evaluacion);
        }

        public async Task DeleteAsync(Guid publicId)
        {
            var evaluacion = await _context.PlaneacionEvaluaciones.Include(x => x.PlaneacionUnidad).ThenInclude(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);
            if (evaluacion == null)
                throw new InvalidOperationException("Evaluación no encontrada.");
            PlaneacionLegacySupport.ExigirMutable(evaluacion.PlaneacionUnidad.PlaneacionDidactica);

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

        public async Task<PlaneacionSecuenciaDto> GetByIdAsync(Guid publicId)
        {
            var secuencia = await _context.PlaneacionSecuencias.AsNoTracking()
                .Include(x => x.PlaneacionUnidad)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);

            if (secuencia == null)
                throw new InvalidOperationException("Secuencia no encontrada.");

            return _mapper.Map<PlaneacionSecuenciaDto>(secuencia);
        }

        public async Task<List<PlaneacionSecuenciaDto>> GetByUnidadIdAsync(Guid unidadPublicId)
        {
            var secuencias = await _context.PlaneacionSecuencias
                .AsNoTracking()
                .Include(x => x.PlaneacionUnidad)
                .Where(x => x.PlaneacionUnidad.PublicId == unidadPublicId && x.Activo)
                .OrderBy(x => x.Orden)
                .ToListAsync();

            return _mapper.Map<List<PlaneacionSecuenciaDto>>(secuencias);
        }

        public async Task<PlaneacionSecuenciaDto> CreateAsync(Guid unidadPublicId, CreatePlaneacionSecuenciaDto dto)
        {
            var unidad = await PlaneacionLegacySupport.BuscarUnidadAsync(_context, unidadPublicId);
            PlaneacionLegacySupport.ExigirMutable(unidad.PlaneacionDidactica);

            // Validar que la estrategia sea válida para la fase seleccionada
            if (!EConverter.IsValidStrategyForPhase(dto.Fase, dto.Estrategia))
                throw new InvalidOperationException(
                    $"La estrategia con valor {dto.Estrategia} no es válida para la fase {dto.Fase}. " +
                    $"Seleccione una estrategia correspondiente a la fase elegida.");

            var secuencia = _mapper.Map<PlaneacionSecuencia>(dto);
            secuencia.PlaneacionUnidad = unidad;

            _context.PlaneacionSecuencias.Add(secuencia);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionSecuenciaDto>(secuencia);
        }

        public async Task<PlaneacionSecuenciaDto> UpdateAsync(Guid publicId, UpdatePlaneacionSecuenciaDto dto)
        {
            var secuencia = await _context.PlaneacionSecuencias.Include(x => x.PlaneacionUnidad).ThenInclude(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);
            if (secuencia == null)
                throw new InvalidOperationException("Secuencia no encontrada.");
            PlaneacionLegacySupport.ExigirMutable(secuencia.PlaneacionUnidad.PlaneacionDidactica);

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

        public async Task DeleteAsync(Guid publicId)
        {
            var secuencia = await _context.PlaneacionSecuencias.Include(x => x.PlaneacionUnidad).ThenInclude(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);
            if (secuencia == null)
                throw new InvalidOperationException("Secuencia no encontrada.");
            PlaneacionLegacySupport.ExigirMutable(secuencia.PlaneacionUnidad.PlaneacionDidactica);

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

        public async Task<PlaneacionReferenciaDto> GetByIdAsync(Guid publicId)
        {
            var referencia = await _context.PlaneacionReferencias.AsNoTracking()
                .Include(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);

            if (referencia == null)
                throw new InvalidOperationException("Referencia no encontrada.");

            return _mapper.Map<PlaneacionReferenciaDto>(referencia);
        }

        public async Task<List<PlaneacionReferenciaDto>> GetByPlaneacionIdAsync(Guid planeacionPublicId)
        {
            var referencias = await _context.PlaneacionReferencias
                .AsNoTracking()
                .Include(x => x.PlaneacionDidactica)
                .Where(x => x.PlaneacionDidactica.PublicId == planeacionPublicId && x.Activo)
                .OrderBy(x => x.Orden)
                .ToListAsync();

            return _mapper.Map<List<PlaneacionReferenciaDto>>(referencias);
        }

        public async Task<PlaneacionReferenciaDto> CreateAsync(Guid planeacionPublicId, CreatePlaneacionReferenciaDto dto)
        {
            var planeacion = await PlaneacionLegacySupport.BuscarPlaneacionAsync(_context, planeacionPublicId);
            PlaneacionLegacySupport.ExigirMutable(planeacion);

            var referencia = _mapper.Map<PlaneacionReferencia>(dto);
            referencia.PlaneacionDidactica = planeacion;

            _context.PlaneacionReferencias.Add(referencia);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionReferenciaDto>(referencia);
        }

        public async Task<PlaneacionReferenciaDto> UpdateAsync(Guid publicId, UpdatePlaneacionReferenciaDto dto)
        {
            var referencia = await _context.PlaneacionReferencias.Include(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);
            if (referencia == null)
                throw new InvalidOperationException("Referencia no encontrada.");
            PlaneacionLegacySupport.ExigirMutable(referencia.PlaneacionDidactica);

            _mapper.Map(dto, referencia);
            referencia.FechaUltimaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<PlaneacionReferenciaDto>(referencia);
        }

        public async Task DeleteAsync(Guid publicId)
        {
            var referencia = await _context.PlaneacionReferencias.Include(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo);
            if (referencia == null)
                throw new InvalidOperationException("Referencia no encontrada.");
            PlaneacionLegacySupport.ExigirMutable(referencia.PlaneacionDidactica);

            referencia.Activo = false;
            referencia.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }

    internal static class PlaneacionLegacySupport
    {
        internal static async Task<PlaneacionDidactica> BuscarPlaneacionAsync(AppDbContext context, Guid publicId) =>
            await context.PlaneacionesDidacticas.FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo && x.DeletedAt == null)
            ?? throw new InvalidOperationException("Planeación no encontrada.");

        internal static async Task<PlaneacionUnidad> BuscarUnidadAsync(AppDbContext context, Guid publicId) =>
            await context.PlaneacionUnidades.Include(x => x.PlaneacionDidactica)
                .FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo && x.DeletedAt == null)
            ?? throw new InvalidOperationException("Unidad no encontrada.");

        internal static async Task<ProgramaAsignatura> BuscarProgramaAsync(AppDbContext context, Guid publicId) =>
            await context.ProgramasAsignatura.FirstOrDefaultAsync(x => x.PublicId == publicId && x.Activo && x.DeletedAt == null)
            ?? throw new InvalidOperationException("Programa de asignatura no encontrado.");

        internal static void ExigirMutable(PlaneacionDidactica planeacion)
        {
            if (planeacion.Estado is EstadoPlaneacion.Aprobada or EstadoPlaneacion.Rechazada or EstadoPlaneacion.Finalizada)
                throw new InvalidOperationException("No se puede modificar una planeación aprobada, rechazada o finalizada.");
        }
    }
}
