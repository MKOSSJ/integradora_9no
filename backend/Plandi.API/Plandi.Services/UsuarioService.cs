using Microsoft.EntityFrameworkCore;
using Plandi.Dto;
using Plandi.Dto.Common;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;
using System.Data;

namespace Plandi.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _dBContext;

        public UsuarioService(AppDbContext dbContext)
        {
            _dBContext = dbContext;
        }
        public async Task<PagedResult<UsuarioResponseDto>> GetAllUsers(int page, int pageSize)
        {
            var query = _dBContext.Usuarios.AsNoTracking().OrderBy(u => u.ApellidoPaterno).ThenBy(u => u.Nombre);
            var total = await query.CountAsync();
            var usuarios = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .Select(u => new UsuarioResponseDto
                {
                    PublicId = u.PublicId,
                    Nombre = u.Nombre,
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    UltimoAcceso = u.UltimoAcceso,
                    Activo = u.Activo && u.DeletedAt == null,
                    CredencialesCompletas = u.Email != null && u.PasswordHash != null
                })
                .ToListAsync();

            return new PagedResult<UsuarioResponseDto> { Items = usuarios, Page = page, PageSize = pageSize, TotalItems = total };
        }
    }
}
