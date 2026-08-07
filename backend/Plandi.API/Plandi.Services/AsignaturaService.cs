using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services
{
    public class AsignaturaService : IAsignaturaService
    {
        private readonly AppDbContext _dbContext;

        public AsignaturaService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<AsignaturaResponseDto>> GetAll()
        {
            var asignaturas = await _dbContext.Asignaturas
                .AsNoTracking()
                .Include(a => a.Academia)
                .Where(a => a.Activo && a.DeletedAt == null)
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            return asignaturas.Select(ToDto).ToList();
        }

        public async Task<AsignaturaResponseDto> GetById(Guid publicId)
        {
            var asignatura = await GetEntity(publicId);
            return ToDto(asignatura);
        }

        public async Task<AsignaturaResponseDto> Create(AsignaturaRequestDto request)
        {
            await ValidateClaveUnica(request.Clave, null);

            long? academiaId = null;
            if (request.AcademiaPublicId.HasValue)
            {
                academiaId = await ResolveAcademiaId(request.AcademiaPublicId.Value);
            }

            var asignatura = new Asignatura
            {
                Nombre = request.Nombre,
                Clave = request.Clave,
                Cuatrimestre = request.Cuatrimestre,
                HorasTotales = request.HorasTotales,
                HorasSemana = request.HorasSemana,
                Creditos = request.Creditos,
                AcademiaId = academiaId
            };

            _dbContext.Asignaturas.Add(asignatura);
            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(asignatura).Reference(a => a.Academia).LoadAsync();

            return ToDto(asignatura);
        }

        public async Task<AsignaturaResponseDto> Update(Guid publicId, AsignaturaRequestDto request)
        {
            var asignatura = await GetEntity(publicId);

            await ValidateClaveUnica(request.Clave, publicId);

            long? academiaId = null;
            if (request.AcademiaPublicId.HasValue)
            {
                academiaId = await ResolveAcademiaId(request.AcademiaPublicId.Value);
            }

            asignatura.Nombre = request.Nombre;
            asignatura.Clave = request.Clave;
            asignatura.Cuatrimestre = request.Cuatrimestre;
            asignatura.HorasTotales = request.HorasTotales;
            asignatura.HorasSemana = request.HorasSemana;
            asignatura.Creditos = request.Creditos;
            asignatura.AcademiaId = academiaId;
            asignatura.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(asignatura).Reference(a => a.Academia).LoadAsync();

            return ToDto(asignatura);
        }

        public async Task<bool> Delete(Guid publicId)
        {
            var asignatura = await GetEntity(publicId);

            asignatura.Activo = false;
            asignatura.DeletedAt = DateTime.UtcNow;
            asignatura.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        private async Task<Asignatura> GetEntity(Guid publicId)
        {
            return await _dbContext.Asignaturas
                .Include(a => a.Academia)
                .FirstOrDefaultAsync(a => a.PublicId == publicId && a.Activo && a.DeletedAt == null)
                ?? throw new AppException("La asignatura especificada no existe.");
        }

        private async Task<long> ResolveAcademiaId(Guid academiaPublicId)
        {
            var academia = await _dbContext.Academias
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.PublicId == academiaPublicId && a.Activo && a.DeletedAt == null);
            if (academia == null)
            {
                throw new AppException("La academia especificada no existe.");
            }
            return academia.Id;
        }

        private async Task ValidateClaveUnica(string clave, Guid? currentPublicId)
        {
            var existe = await _dbContext.Asignaturas
                .AnyAsync(a => a.Clave == clave && a.Activo && a.DeletedAt == null
                    && (!currentPublicId.HasValue || a.PublicId != currentPublicId.Value));
            if (existe)
            {
                throw new AppException("Ya existe una asignatura con esa clave.");
            }
        }

        private static AsignaturaResponseDto ToDto(Asignatura asignatura)
        {
            return new AsignaturaResponseDto
            {
                PublicId = asignatura.PublicId,
                AcademiaPublicId = asignatura.Academia?.PublicId,
                Nombre = asignatura.Nombre,
                Clave = asignatura.Clave,
                Cuatrimestre = asignatura.Cuatrimestre,
                HorasTotales = asignatura.HorasTotales,
                HorasSemana = asignatura.HorasSemana,
                Creditos = asignatura.Creditos,
                Activo = asignatura.Activo
            };
        }
    }
}
