using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services
{
    public class GrupoService : IGrupoService
    {
        private readonly AppDbContext _dbContext;

        public GrupoService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<GrupoResponseDto>> GetAll()
        {
            var grupos = await _dbContext.Grupos
                .AsNoTracking()
                .Include(g => g.Carrera)
                .Include(g => g.Periodo)
                .Where(g => g.Activo && g.DeletedAt == null)
                .OrderBy(g => g.Nombre)
                .ToListAsync();

            return grupos.Select(ToDto).ToList();
        }

        public async Task<GrupoResponseDto> GetById(Guid publicId)
        {
            var grupo = await GetEntity(publicId);
            return ToDto(grupo);
        }

        public async Task<GrupoResponseDto> Create(GrupoRequestDto request)
        {
            var carreraId = await ResolveCarreraId(request.CarreraPublicId);
            var periodoId = await ResolvePeriodoId(request.PeriodoPublicId);

            await ValidateNombreUnico(periodoId, request.Nombre, null);

            var grupo = new Grupo
            {
                Nombre = request.Nombre,
                Cuatrimestre = request.Cuatrimestre,
                CarreraId = carreraId,
                PeriodoId = periodoId
            };

            _dbContext.Grupos.Add(grupo);
            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(grupo).Reference(g => g.Carrera).LoadAsync();
            await _dbContext.Entry(grupo).Reference(g => g.Periodo).LoadAsync();

            return ToDto(grupo);
        }

        public async Task<GrupoResponseDto> Update(Guid publicId, GrupoRequestDto request)
        {
            var grupo = await GetEntity(publicId);

            var carreraId = await ResolveCarreraId(request.CarreraPublicId);
            var periodoId = await ResolvePeriodoId(request.PeriodoPublicId);

            await ValidateNombreUnico(periodoId, request.Nombre, publicId);

            grupo.Nombre = request.Nombre;
            grupo.Cuatrimestre = request.Cuatrimestre;
            grupo.CarreraId = carreraId;
            grupo.PeriodoId = periodoId;
            grupo.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(grupo).Reference(g => g.Carrera).LoadAsync();
            await _dbContext.Entry(grupo).Reference(g => g.Periodo).LoadAsync();

            return ToDto(grupo);
        }

        public async Task<bool> Delete(Guid publicId)
        {
            var grupo = await GetEntity(publicId);

            grupo.Activo = false;
            grupo.DeletedAt = DateTime.UtcNow;
            grupo.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        private async Task<Grupo> GetEntity(Guid publicId)
        {
            return await _dbContext.Grupos
                .Include(g => g.Carrera)
                .Include(g => g.Periodo)
                .FirstOrDefaultAsync(g => g.PublicId == publicId && g.Activo && g.DeletedAt == null)
                ?? throw new AppException("El grupo especificado no existe.");
        }

        private async Task<long> ResolveCarreraId(Guid carreraPublicId)
        {
            var carrera = await _dbContext.Carreras
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.PublicId == carreraPublicId && c.Activo && c.DeletedAt == null);
            if (carrera == null)
            {
                throw new AppException("La carrera especificada no existe.");
            }
            return carrera.Id;
        }

        private async Task<long> ResolvePeriodoId(Guid periodoPublicId)
        {
            var periodo = await _dbContext.Periodos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PublicId == periodoPublicId && p.Activo && p.DeletedAt == null);
            if (periodo == null)
            {
                throw new AppException("El periodo especificado no existe.");
            }
            return periodo.Id;
        }

        private async Task ValidateNombreUnico(long periodoId, string nombre, Guid? currentPublicId)
        {
            var existe = await _dbContext.Grupos
                .AnyAsync(g => g.PeriodoId == periodoId && g.Nombre == nombre && g.Activo && g.DeletedAt == null
                    && (!currentPublicId.HasValue || g.PublicId != currentPublicId.Value));
            if (existe)
            {
                throw new AppException("Ya existe un grupo con ese nombre dentro del periodo.");
            }
        }

        private static GrupoResponseDto ToDto(Grupo grupo)
        {
            return new GrupoResponseDto
            {
                PublicId = grupo.PublicId,
                CarreraPublicId = grupo.Carrera?.PublicId ?? Guid.Empty,
                PeriodoPublicId = grupo.Periodo?.PublicId ?? Guid.Empty,
                Nombre = grupo.Nombre,
                Cuatrimestre = grupo.Cuatrimestre,
                Activo = grupo.Activo
            };
        }
    }
}
