using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;
using Plandi.Dto.Enums;

namespace Plandi.Services
{
    public class PeriodoService : IPeriodoService
    {
        private readonly AppDbContext _dbContext;

        private readonly IPeriodoLifecycleService _lifecycle;

        public PeriodoService(AppDbContext dbContext, IPeriodoLifecycleService lifecycle)
        {
            _dbContext = dbContext;
            _lifecycle = lifecycle;
        }

        public async Task<IEnumerable<PeriodoResponseDto>> GetAll()
        {
            var periodos = await _dbContext.Periodos
                .AsNoTracking()
                .Include(p => p.CicloEscolar)
                .Where(p => p.Activo && p.DeletedAt == null)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return periodos.Select(ToDto).ToList();
        }

        public async Task<PeriodoResponseDto> GetById(Guid publicId)
        {
            var periodo = await GetEntity(publicId);
            return ToDto(periodo);
        }

        public async Task<PeriodoResponseDto> Create(PeriodoRequestDto request, long actorId)
        {
            var cicloEscolarId = await ResolveCicloEscolarId(request.CicloEscolarPublicId);

            ValidateFechas(request);
            await ValidateNombreUnico(cicloEscolarId, request.Nombre, null);

            var periodo = new Periodo
            {
                CicloEscolarId = cicloEscolarId,
                Nombre = request.Nombre,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                CreatedBy = actorId
            };
            periodo.Estado = _lifecycle.ObtenerEstadoEfectivo(periodo);
            if (periodo.Estado == EstadoPeriodo.Cerrado) periodo.FechaCierre = DateTime.UtcNow;

            _dbContext.Periodos.Add(periodo);
            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(periodo).Reference(p => p.CicloEscolar).LoadAsync();

            return ToDto(periodo);
        }

        public async Task<PeriodoResponseDto> Update(Guid publicId, PeriodoRequestDto request, long actorId)
        {
            var periodo = await GetEntity(publicId);
            await _lifecycle.ExigirEditableAsync(periodo.Id);

            var cicloEscolarId = await ResolveCicloEscolarId(request.CicloEscolarPublicId);

            ValidateFechas(request);
            await ValidateNombreUnico(cicloEscolarId, request.Nombre, publicId);

            periodo.CicloEscolarId = cicloEscolarId;
            periodo.Nombre = request.Nombre;
            periodo.FechaInicio = request.FechaInicio;
            periodo.FechaFin = request.FechaFin;
            periodo.Estado = _lifecycle.ObtenerEstadoEfectivo(periodo);
            if (periodo.Estado == EstadoPeriodo.Cerrado) periodo.FechaCierre ??= DateTime.UtcNow;
            periodo.UpdatedAt = DateTime.UtcNow;
            periodo.UpdatedBy = actorId;

            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(periodo).Reference(p => p.CicloEscolar).LoadAsync();

            return ToDto(periodo);
        }

        public async Task<bool> Delete(Guid publicId, long actorId)
        {
            var periodo = await GetEntity(publicId);
            await _lifecycle.ExigirEditableAsync(periodo.Id);

            periodo.Activo = false;
            periodo.DeletedAt = DateTime.UtcNow;
            periodo.UpdatedAt = DateTime.UtcNow;
            periodo.UpdatedBy = actorId;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<PeriodoResponseDto> Cerrar(Guid publicId, long actorId, CancellationToken cancellationToken = default)
        {
            var periodo = await _dbContext.Periodos.Include(x => x.CicloEscolar)
                .SingleOrDefaultAsync(x => x.PublicId == publicId && x.Activo && x.DeletedAt == null, cancellationToken)
                ?? throw new NotFoundException("El periodo especificado no existe.");
            await _lifecycle.CerrarAsync(periodo.Id, actorId, cancellationToken);
            periodo.Estado = EstadoPeriodo.Cerrado;
            return ToDto(periodo);
        }

        private async Task<Periodo> GetEntity(Guid publicId)
        {
            return await _dbContext.Periodos
                .Include(p => p.CicloEscolar)
                .FirstOrDefaultAsync(p => p.PublicId == publicId && p.Activo && p.DeletedAt == null)
                ?? throw new AppException("El periodo especificado no existe.");
        }

        private async Task<long> ResolveCicloEscolarId(Guid cicloEscolarPublicId)
        {
            var cicloEscolar = await _dbContext.CiclosEscolares
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.PublicId == cicloEscolarPublicId && c.Activo && c.DeletedAt == null);
            if (cicloEscolar == null)
            {
                throw new AppException("El ciclo escolar especificado no existe.");
            }
            return cicloEscolar.Id;
        }

        private async Task ValidateNombreUnico(long cicloEscolarId, string nombre, Guid? currentPublicId)
        {
            var existe = await _dbContext.Periodos
                .AnyAsync(p => p.CicloEscolarId == cicloEscolarId && p.Nombre == nombre && p.Activo && p.DeletedAt == null
                    && (!currentPublicId.HasValue || p.PublicId != currentPublicId.Value));
            if (existe)
            {
                throw new AppException("Ya existe un periodo con ese nombre dentro del ciclo escolar.");
            }
        }

        private static void ValidateFechas(PeriodoRequestDto request)
        {
            if (request.FechaFin <= request.FechaInicio)
            {
                throw new AppException("La fecha de fin debe ser posterior a la fecha de inicio.");
            }
        }

        private PeriodoResponseDto ToDto(Periodo periodo)
        {
            var estadoEfectivo = _lifecycle.ObtenerEstadoEfectivo(periodo);
            return new PeriodoResponseDto
            {
                PublicId = periodo.PublicId,
                CicloEscolarPublicId = periodo.CicloEscolar?.PublicId ?? Guid.Empty,
                Nombre = periodo.Nombre,
                FechaInicio = periodo.FechaInicio,
                FechaFin = periodo.FechaFin,
                Activo = periodo.Activo,
                Estado = periodo.Estado,
                EstadoEfectivo = estadoEfectivo,
                FechaCierre = periodo.FechaCierre,
                PermiteModificaciones = _lifecycle.PermiteModificaciones(periodo)
            };
        }
    }
}
