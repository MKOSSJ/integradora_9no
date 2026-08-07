using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services
{
    public class CargaAcademicaService : ICargaAcademicaService
    {
        private readonly AppDbContext _dbContext;

        public CargaAcademicaService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CargaAcademicaResponseDto>> GetAll()
        {
            var cargas = await _dbContext.CargasAcademicas
                .AsNoTracking()
                .Include(c => c.Periodo)
                .Include(c => c.Grupo)
                .Include(c => c.Asignatura)
                .Include(c => c.Docente)
                .Include(c => c.Revisor)
                .Include(c => c.Academia)
                .Where(c => c.Activo && c.DeletedAt == null)
                .OrderBy(c => c.Id)
                .ToListAsync();

            return cargas.Select(ToDto).ToList();
        }

        public async Task<CargaAcademicaResponseDto> GetById(Guid publicId)
        {
            var carga = await GetEntity(publicId);
            return ToDto(carga);
        }

        public async Task<CargaAcademicaResponseDto> Create(CargaAcademicaRequestDto request)
        {
            var periodoId = await ResolvePeriodoId(request.PeriodoPublicId);
            var grupoId = await ResolveGrupoId(request.GrupoPublicId);
            var asignaturaId = await ResolveAsignaturaId(request.AsignaturaPublicId);
            var docenteId = await ResolveDocenteId(request.DocentePublicId);
            long? revisorId = request.RevisorPublicId.HasValue
                ? await ResolveRevisorId(request.RevisorPublicId.Value)
                : null;
            long? academiaId = request.AcademiaPublicId.HasValue
                ? await ResolveAcademiaId(request.AcademiaPublicId.Value)
                : null;

            await ValidateNoDuplicada(periodoId, grupoId, asignaturaId, docenteId, null);

            var carga = new CargaAcademica
            {
                PeriodoId = periodoId,
                GrupoId = grupoId,
                AsignaturaId = asignaturaId,
                DocenteId = docenteId,
                RevisorId = revisorId,
                AcademiaId = academiaId
            };

            _dbContext.CargasAcademicas.Add(carga);
            await _dbContext.SaveChangesAsync();
            await LoadReferences(carga);

            return ToDto(carga);
        }

        public async Task<CargaAcademicaResponseDto> Update(Guid publicId, CargaAcademicaRequestDto request)
        {
            var carga = await GetEntity(publicId);

            var periodoId = await ResolvePeriodoId(request.PeriodoPublicId);
            var grupoId = await ResolveGrupoId(request.GrupoPublicId);
            var asignaturaId = await ResolveAsignaturaId(request.AsignaturaPublicId);
            var docenteId = await ResolveDocenteId(request.DocentePublicId);
            long? revisorId = request.RevisorPublicId.HasValue
                ? await ResolveRevisorId(request.RevisorPublicId.Value)
                : null;
            long? academiaId = request.AcademiaPublicId.HasValue
                ? await ResolveAcademiaId(request.AcademiaPublicId.Value)
                : null;

            await ValidateNoDuplicada(periodoId, grupoId, asignaturaId, docenteId, publicId);

            carga.PeriodoId = periodoId;
            carga.GrupoId = grupoId;
            carga.AsignaturaId = asignaturaId;
            carga.DocenteId = docenteId;
            carga.RevisorId = revisorId;
            carga.AcademiaId = academiaId;
            carga.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await LoadReferences(carga);

            return ToDto(carga);
        }

        public async Task<bool> Delete(Guid publicId)
        {
            var carga = await GetEntity(publicId);

            carga.Activo = false;
            carga.DeletedAt = DateTime.UtcNow;
            carga.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        private async Task<CargaAcademica> GetEntity(Guid publicId)
        {
            return await _dbContext.CargasAcademicas
                .Include(c => c.Periodo)
                .Include(c => c.Grupo)
                .Include(c => c.Asignatura)
                .Include(c => c.Docente)
                .Include(c => c.Revisor)
                .Include(c => c.Academia)
                .FirstOrDefaultAsync(c => c.PublicId == publicId && c.Activo && c.DeletedAt == null)
                ?? throw new AppException("La carga académica especificada no existe.");
        }

        private async Task LoadReferences(CargaAcademica carga)
        {
            await _dbContext.Entry(carga).Reference(c => c.Periodo).LoadAsync();
            await _dbContext.Entry(carga).Reference(c => c.Grupo).LoadAsync();
            await _dbContext.Entry(carga).Reference(c => c.Asignatura).LoadAsync();
            await _dbContext.Entry(carga).Reference(c => c.Docente).LoadAsync();
            await _dbContext.Entry(carga).Reference(c => c.Revisor).LoadAsync();
            await _dbContext.Entry(carga).Reference(c => c.Academia).LoadAsync();
        }

        private async Task ValidateNoDuplicada(long periodoId, long grupoId, long asignaturaId, long docenteId, Guid? currentPublicId)
        {
            var existe = await _dbContext.CargasAcademicas
                .AnyAsync(c => c.PeriodoId == periodoId && c.GrupoId == grupoId && c.AsignaturaId == asignaturaId
                    && c.DocenteId == docenteId && c.Activo && c.DeletedAt == null
                    && (!currentPublicId.HasValue || c.PublicId != currentPublicId.Value));
            if (existe)
            {
                throw new AppException("Ya existe una carga académica con el mismo periodo, grupo, asignatura y docente.");
            }
        }

        private async Task<long> ResolvePeriodoId(Guid publicId)
        {
            var entidad = await _dbContext.Periodos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PublicId == publicId && p.Activo && p.DeletedAt == null);
            if (entidad == null)
            {
                throw new AppException("El periodo especificado no existe.");
            }
            return entidad.Id;
        }

        private async Task<long> ResolveGrupoId(Guid publicId)
        {
            var entidad = await _dbContext.Grupos
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.PublicId == publicId && g.Activo && g.DeletedAt == null);
            if (entidad == null)
            {
                throw new AppException("El grupo especificado no existe.");
            }
            return entidad.Id;
        }

        private async Task<long> ResolveAsignaturaId(Guid publicId)
        {
            var entidad = await _dbContext.Asignaturas
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.PublicId == publicId && a.Activo && a.DeletedAt == null);
            if (entidad == null)
            {
                throw new AppException("La asignatura especificada no existe.");
            }
            return entidad.Id;
        }

        private async Task<long> ResolveDocenteId(Guid publicId)
        {
            var entidad = await _dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PublicId == publicId && u.Activo && u.DeletedAt == null);
            if (entidad == null)
            {
                throw new AppException("El docente especificado no existe.");
            }
            return entidad.Id;
        }

        private async Task<long> ResolveRevisorId(Guid publicId)
        {
            var entidad = await _dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PublicId == publicId && u.Activo && u.DeletedAt == null);
            if (entidad == null)
            {
                throw new AppException("El revisor especificado no existe.");
            }
            return entidad.Id;
        }

        private async Task<long> ResolveAcademiaId(Guid publicId)
        {
            var entidad = await _dbContext.Academias
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.PublicId == publicId && a.Activo && a.DeletedAt == null);
            if (entidad == null)
            {
                throw new AppException("La academia especificada no existe.");
            }
            return entidad.Id;
        }

        private static CargaAcademicaResponseDto ToDto(CargaAcademica carga)
        {
            return new CargaAcademicaResponseDto
            {
                PublicId = carga.PublicId,
                PeriodoPublicId = carga.Periodo?.PublicId ?? Guid.Empty,
                GrupoPublicId = carga.Grupo?.PublicId ?? Guid.Empty,
                AsignaturaPublicId = carga.Asignatura?.PublicId ?? Guid.Empty,
                DocentePublicId = carga.Docente?.PublicId ?? Guid.Empty,
                RevisorPublicId = carga.Revisor?.PublicId,
                AcademiaPublicId = carga.Academia?.PublicId,
                Activo = carga.Activo
            };
        }
    }
}
