using Microsoft.EntityFrameworkCore;
using Plandi.Dto;
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
        public async Task<IEnumerable<UsuarioResponseDto>> GetAllUsers()
        {
            var usuarios = await _dBContext.Usuarios
                .AsNoTracking()
                .Select(u => new UsuarioResponseDto
                {
                    Nombre = u.Nombre,
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    UltimoAcceso = u.UltimoAcceso
                })
                .ToListAsync();

            return usuarios;
        }
    }
}
