using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Catalogos;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services
{
    public class AcademiaService : IAcademiaService
    {
        private readonly AppDbContext _dbContext;

        public AcademiaService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<AcademiaResponseDto>> GetAll()
        {
            var academias = await _dbContext.Academias
                .AsNoTracking()
                .Where(a => a.Activo && a.DeletedAt == null)
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            return academias.Select(ToDto).ToList();
        }

        public async Task<AcademiaResponseDto> GetById(Guid publicId)
        {
            var academia = await GetEntity(publicId);
            return ToDto(academia);
        }

        public async Task<AcademiaResponseDto> Create(AcademiaRequestDto request)
        {
            await ValidateNombreUnico(request.Nombre, null);

            var academia = new Academia
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion
            };

            _dbContext.Academias.Add(academia);
            await _dbContext.SaveChangesAsync();

            return ToDto(academia);
        }

        public async Task<AcademiaResponseDto> Update(Guid publicId, AcademiaRequestDto request)
        {
            var academia = await GetEntity(publicId);

            await ValidateNombreUnico(request.Nombre, publicId);

            academia.Nombre = request.Nombre;
            academia.Descripcion = request.Descripcion;
            academia.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return ToDto(academia);
        }

        public async Task<bool> Delete(Guid publicId)
        {
            var academia = await GetEntity(publicId);

            academia.Activo = false;
            academia.DeletedAt = DateTime.UtcNow;
            academia.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<AcademiaUsuarioResponseDto>> GetUsuarios(Guid academiaPublicId)
        {
            var academia = await GetEntity(academiaPublicId);

            var vinculados = await _dbContext.AcademiaUsuarios
                .AsNoTracking()
                .Include(v => v.Usuario)
                .Where(v => v.AcademiaId == academia.Id && v.Activo)
                .ToListAsync();

            return vinculados.Select(ToDto).ToList();
        }

        public async Task<AcademiaUsuarioResponseDto> AsignarUsuario(Guid academiaPublicId, AcademiaUsuarioRequestDto request)
        {
            var academia = await GetEntity(academiaPublicId);

            var usuario = await _dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PublicId == request.UsuarioPublicId && u.Activo && u.DeletedAt == null)
                ?? throw new AppException("El usuario especificado no existe.");

            var vinculo = await _dbContext.AcademiaUsuarios
                .FirstOrDefaultAsync(v => v.AcademiaId == academia.Id && v.UsuarioId == usuario.Id);

            if (vinculo == null)
            {
                vinculo = new AcademiaUsuario
                {
                    AcademiaId = academia.Id,
                    UsuarioId = usuario.Id,
                    Rol = request.Rol
                };
                _dbContext.AcademiaUsuarios.Add(vinculo);
            }
            else
            {
                if (vinculo.Activo)
                {
                    throw new AppException("El usuario ya está asignado a la academia.");
                }

                vinculo.Activo = true;
                vinculo.Rol = request.Rol;
                vinculo.CreatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(vinculo).Reference(v => v.Usuario).LoadAsync();

            return ToDto(vinculo);
        }

        public async Task<bool> DesasignarUsuario(Guid academiaPublicId, Guid usuarioPublicId)
        {
            var academia = await GetEntity(academiaPublicId);

            var usuario = await _dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PublicId == usuarioPublicId && u.Activo && u.DeletedAt == null)
                ?? throw new AppException("El usuario especificado no existe.");

            var vinculo = await _dbContext.AcademiaUsuarios
                .FirstOrDefaultAsync(v => v.AcademiaId == academia.Id && v.UsuarioId == usuario.Id);

            if (vinculo == null || !vinculo.Activo)
            {
                throw new AppException("El usuario no está asignado a la academia.");
            }

            vinculo.Activo = false;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        private async Task<Academia> GetEntity(Guid publicId)
        {
            return await _dbContext.Academias
                .FirstOrDefaultAsync(a => a.PublicId == publicId && a.Activo && a.DeletedAt == null)
                ?? throw new AppException("La academia especificada no existe.");
        }

        private async Task ValidateNombreUnico(string nombre, Guid? currentPublicId)
        {
            var existe = await _dbContext.Academias
                .AnyAsync(a => a.Nombre == nombre && a.Activo && a.DeletedAt == null
                    && (!currentPublicId.HasValue || a.PublicId != currentPublicId.Value));
            if (existe)
            {
                throw new AppException("Ya existe una academia con ese nombre.");
            }
        }

        private static AcademiaResponseDto ToDto(Academia academia)
        {
            return new AcademiaResponseDto
            {
                PublicId = academia.PublicId,
                Nombre = academia.Nombre,
                Descripcion = academia.Descripcion,
                Activo = academia.Activo
            };
        }

        private static AcademiaUsuarioResponseDto ToDto(AcademiaUsuario vinculo)
        {
            return new AcademiaUsuarioResponseDto
            {
                UsuarioPublicId = vinculo.Usuario?.PublicId ?? Guid.Empty,
                UsuarioNombre = vinculo.Usuario == null
                    ? null
                    : $"{vinculo.Usuario.Nombre} {vinculo.Usuario.ApellidoPaterno}".Trim(),
                Rol = vinculo.Rol,
                Activo = vinculo.Activo
            };
        }
    }
}
