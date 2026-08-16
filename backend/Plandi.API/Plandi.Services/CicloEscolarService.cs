using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services
{
    public class CicloEscolarService : ICicloEscolarService
    {
        private readonly AppDbContext _dbContext;

        public CicloEscolarService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CicloEscolarResponseDto>> GetAll()
        {
            var ciclosEscolares = await _dbContext.CiclosEscolares
                .AsNoTracking()
                .Where(c => c.Activo && c.DeletedAt == null)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return ciclosEscolares.Select(ToDto).ToList();
        }

        public async Task<CicloEscolarResponseDto> GetById(Guid publicId)
        {
            var cicloEscolar = await GetEntity(publicId);
            return ToDto(cicloEscolar);
        }

        public async Task<CicloEscolarResponseDto> Create(CicloEscolarRequestDto request, long actorId)
        {
            ValidateFechas(request);
            await ValidateNombreUnico(request.Nombre, null);

            var cicloEscolar = new CicloEscolar
            {
                Nombre = request.Nombre,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                CreatedBy = actorId
            };

            _dbContext.CiclosEscolares.Add(cicloEscolar);
            await _dbContext.SaveChangesAsync();

            return ToDto(cicloEscolar);
        }

        public async Task<CicloEscolarResponseDto> Update(Guid publicId, CicloEscolarRequestDto request, long actorId)
        {
            var cicloEscolar = await GetEntity(publicId);

            ValidateFechas(request);
            await ValidateNombreUnico(request.Nombre, publicId);

            cicloEscolar.Nombre = request.Nombre;
            cicloEscolar.FechaInicio = request.FechaInicio;
            cicloEscolar.FechaFin = request.FechaFin;
            cicloEscolar.UpdatedAt = DateTime.UtcNow;
            cicloEscolar.UpdatedBy = actorId;

            await _dbContext.SaveChangesAsync();

            return ToDto(cicloEscolar);
        }

        public async Task<bool> Delete(Guid publicId, long actorId)
        {
            var cicloEscolar = await GetEntity(publicId);

            cicloEscolar.Activo = false;
            cicloEscolar.DeletedAt = DateTime.UtcNow;
            cicloEscolar.UpdatedAt = DateTime.UtcNow;
            cicloEscolar.UpdatedBy = actorId;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        private async Task<CicloEscolar> GetEntity(Guid publicId)
        {
            return await _dbContext.CiclosEscolares
                .FirstOrDefaultAsync(c => c.PublicId == publicId && c.Activo && c.DeletedAt == null)
                ?? throw new AppException("El ciclo escolar especificado no existe.");
        }

        private async Task ValidateNombreUnico(string nombre, Guid? currentPublicId)
        {
            var existe = await _dbContext.CiclosEscolares
                .AnyAsync(c => c.Nombre == nombre && c.Activo && c.DeletedAt == null
                    && (!currentPublicId.HasValue || c.PublicId != currentPublicId.Value));
            if (existe)
            {
                throw new AppException("Ya existe un ciclo escolar con ese nombre.");
            }
        }

        private static void ValidateFechas(CicloEscolarRequestDto request)
        {
            if (request.FechaFin <= request.FechaInicio)
            {
                throw new AppException("La fecha de fin debe ser posterior a la fecha de inicio.");
            }
        }

        private static CicloEscolarResponseDto ToDto(CicloEscolar cicloEscolar)
        {
            return new CicloEscolarResponseDto
            {
                PublicId = cicloEscolar.PublicId,
                Nombre = cicloEscolar.Nombre,
                FechaInicio = cicloEscolar.FechaInicio,
                FechaFin = cicloEscolar.FechaFin,
                Activo = cicloEscolar.Activo
            };
        }
    }
}
