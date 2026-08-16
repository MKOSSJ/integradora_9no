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
        private readonly IPeriodoLifecycleService _lifecycle;

        public CargaAcademicaService(AppDbContext dbContext, IPeriodoLifecycleService lifecycle)
        {
            _dbContext = dbContext;
            _lifecycle = lifecycle;
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

        public async Task<CargaAcademicaResponseDto> Create(CargaAcademicaRequestDto request, long actorId)
        {
            var periodoId = await ResolvePeriodoId(request.PeriodoPublicId);
            await _lifecycle.ExigirEditableAsync(periodoId);
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
            await ValidateCoherencia(periodoId, grupoId, asignaturaId, academiaId);

            var carga = new CargaAcademica
            {
                PeriodoId = periodoId,
                GrupoId = grupoId,
                AsignaturaId = asignaturaId,
                DocenteId = docenteId,
                RevisorId = revisorId,
                AcademiaId = academiaId,
                CreatedBy = actorId
            };

            _dbContext.CargasAcademicas.Add(carga);
            await _dbContext.SaveChangesAsync();
            await LoadReferences(carga);

            return ToDto(carga);
        }

        public async Task<CargaAcademicaResponseDto> Update(Guid publicId, CargaAcademicaRequestDto request, long actorId)
        {
            var carga = await GetEntity(publicId);
            await _lifecycle.ExigirEditableAsync(carga.PeriodoId);

            var periodoId = await ResolvePeriodoId(request.PeriodoPublicId);
            if (periodoId != carga.PeriodoId) await _lifecycle.ExigirEditableAsync(periodoId);
            var grupoId = await ResolveGrupoId(request.GrupoPublicId);
            var asignaturaId = await ResolveAsignaturaId(request.AsignaturaPublicId);
            var docenteId = await ResolveDocenteId(request.DocentePublicId);
            long? revisorId = request.RevisorPublicId.HasValue
                ? await ResolveRevisorId(request.RevisorPublicId.Value)
                : null;
            long? academiaId = request.AcademiaPublicId.HasValue
                ? await ResolveAcademiaId(request.AcademiaPublicId.Value)
                : null;

            if (periodoId != carga.PeriodoId || asignaturaId != carga.AsignaturaId || docenteId != carga.DocenteId)
                throw new AppException("El periodo, la asignatura y el docente no pueden cambiarse mediante la actualización general. Utilice una operación administrativa específica.");

            await ValidateNoDuplicada(periodoId, grupoId, asignaturaId, docenteId, publicId);
            await ValidateCoherencia(periodoId, grupoId, asignaturaId, academiaId);

            carga.PeriodoId = periodoId;
            carga.GrupoId = grupoId;
            carga.AsignaturaId = asignaturaId;
            carga.DocenteId = docenteId;
            carga.RevisorId = revisorId;
            carga.AcademiaId = academiaId;
            carga.UpdatedAt = DateTime.UtcNow;
            carga.UpdatedBy = actorId;

            await _dbContext.SaveChangesAsync();
            await LoadReferences(carga);

            return ToDto(carga);
        }

        public async Task<bool> Delete(Guid publicId, long actorId)
        {
            var carga = await GetEntity(publicId);
            await _lifecycle.ExigirEditableAsync(carga.PeriodoId);

            carga.Activo = false;
            carga.DeletedAt = DateTime.UtcNow;
            carga.UpdatedAt = DateTime.UtcNow;
            carga.UpdatedBy = actorId;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<CargaAcademicaResponseDto> UpdateGrupo(Guid publicId, ActualizarGrupoCargaAcademicaDto request, long actorId, CancellationToken cancellationToken = default)
        {
            var carga = await GetEntity(publicId);
            await _lifecycle.ExigirEditableAsync(carga.PeriodoId, cancellationToken);
            var grupoId = await ResolveGrupoId(request.GrupoPublicId);
            await ValidateNoDuplicada(carga.PeriodoId, grupoId, carga.AsignaturaId, carga.DocenteId, publicId);
            await ValidateCoherencia(carga.PeriodoId, grupoId, carga.AsignaturaId, carga.AcademiaId);

            carga.GrupoId = grupoId;
            carga.UpdatedAt = DateTime.UtcNow;
            carga.UpdatedBy = actorId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await LoadReferences(carga);
            return ToDto(carga);
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

        private async Task ValidateCoherencia(long periodoId, long grupoId, long asignaturaId, long? academiaId)
        {
            var grupo = await _dbContext.Grupos.AsNoTracking().SingleOrDefaultAsync(g => g.Id == grupoId && g.Activo && g.DeletedAt == null);
            if (grupo is null || grupo.PeriodoId != periodoId)
                throw new AppException("El grupo no pertenece al periodo especificado.");
            var asignatura = await _dbContext.Asignaturas.AsNoTracking().SingleAsync(a => a.Id == asignaturaId);
            if (asignatura.Cuatrimestre != grupo.Cuatrimestre)
                throw new AppException("La asignatura no es compatible con el cuatrimestre del grupo especificado.");
            if (academiaId.HasValue && asignatura.AcademiaId.HasValue && asignatura.AcademiaId != academiaId)
                throw new AppException("La asignatura no pertenece a la academia especificada.");
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
                .FirstOrDefaultAsync(u => u.PublicId == publicId && u.Activo && u.DeletedAt == null &&
                    u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "Docente" && ur.Rol.Activo && ur.Rol.DeletedAt == null));
            if (entidad == null)
            {
                throw new AppException("El docente especificado no existe o no tiene el rol Docente.");
            }
            return entidad.Id;
        }

        private async Task<long> ResolveRevisorId(Guid publicId)
        {
            var entidad = await _dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PublicId == publicId && u.Activo && u.DeletedAt == null &&
                    u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "Revisor" && ur.Rol.Activo && ur.Rol.DeletedAt == null));
            if (entidad == null)
            {
                throw new AppException("El revisor especificado no existe o no tiene el rol Revisor.");
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
