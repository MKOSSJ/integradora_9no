using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services
{
    public class CarreraService : ICarreraService
    {
        private readonly AppDbContext _dbContext;

        public CarreraService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CarreraResponseDto>> GetAll()
        {
            var carreras = await _dbContext.Carreras
                .AsNoTracking()
                .Where(c => c.Activo && c.DeletedAt == null)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return carreras.Select(ToDto).ToList();
        }

        public async Task<CarreraResponseDto> GetById(Guid publicId)
        {
            var carrera = await GetEntity(publicId);
            return ToDto(carrera);
        }

        public async Task<CarreraResponseDto> Create(CarreraRequestDto request)
        {
            await ValidateClaveUnica(request.Clave, null);

            var carrera = new Carrera
            {
                Nombre = request.Nombre,
                Clave = request.Clave,
                Nivel = request.Nivel
            };

            _dbContext.Carreras.Add(carrera);
            await _dbContext.SaveChangesAsync();

            return ToDto(carrera);
        }

        public async Task<CarreraResponseDto> Update(Guid publicId, CarreraRequestDto request)
        {
            var carrera = await GetEntity(publicId);

            await ValidateClaveUnica(request.Clave, publicId);

            carrera.Nombre = request.Nombre;
            carrera.Clave = request.Clave;
            carrera.Nivel = request.Nivel;
            carrera.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return ToDto(carrera);
        }

        public async Task<bool> Delete(Guid publicId)
        {
            var carrera = await GetEntity(publicId);

            carrera.Activo = false;
            carrera.DeletedAt = DateTime.UtcNow;
            carrera.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        private async Task<Carrera> GetEntity(Guid publicId)
        {
            return await _dbContext.Carreras
                .FirstOrDefaultAsync(c => c.PublicId == publicId && c.Activo && c.DeletedAt == null)
                ?? throw new AppException("La carrera especificada no existe.");
        }

        private async Task ValidateClaveUnica(string clave, Guid? currentPublicId)
        {
            var existe = await _dbContext.Carreras
                .AnyAsync(c => c.Clave == clave && c.Activo && c.DeletedAt == null
                    && (!currentPublicId.HasValue || c.PublicId != currentPublicId.Value));
            if (existe)
            {
                throw new AppException("Ya existe una carrera con esa clave.");
            }
        }

        private static CarreraResponseDto ToDto(Carrera carrera)
        {
            return new CarreraResponseDto
            {
                PublicId = carrera.PublicId,
                Nombre = carrera.Nombre,
                Clave = carrera.Clave,
                Nivel = carrera.Nivel,
                Activo = carrera.Activo
            };
        }
    }
}
