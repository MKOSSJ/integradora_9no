using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Auth;
using Plandi.Library.Models;
using Plandi.Dto.Utils;
using Plandi.Services.Interfaces;
using System.Data;
using System.Security.Cryptography;

namespace Plandi.Services
{
    public  class TwoFactorService : ITwoFactorService
    {
        private readonly AppDbContext _dBContext;
        private readonly ITokenService _tokenService;

        public TwoFactorService(AppDbContext dbContext, ITokenService tokenService)
        {
            _dBContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto> VerifyTwoFactorCodeAsync(TwoFactorDto twoFactorDto)
        {
            var emailCleaned = twoFactorDto.email.Trim().ToLower();
            var code = twoFactorDto.Code.Trim();

            var usuario = await _dBContext.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == emailCleaned && u.Activo && u.DeletedAt == null);

            if (usuario == null || usuario.TwoFactorSecretKey != code || usuario.TwoFactorCodeExpires < DateTime.UtcNow)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Código de verificación incorrecto o expirado."
                };
            }

            usuario.TwoFactorSecretKey = null;
            usuario.TwoFactorCodeExpires = null;
            await _dBContext.SaveChangesAsync();

            var (token, expiresAt) = await _tokenService.GenerateAccessToken(usuario);
            var (refreshToken, tokenHash, refreshTokenExpiresAt) = await _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UsuarioId = usuario.Id,
                TokenHash = tokenHash,
                Created = DateTime.UtcNow,
                Expires = refreshTokenExpiresAt
            };

            _dBContext.RefreshTokens.Add(refreshTokenEntity);
            await _dBContext.SaveChangesAsync();

            return new LoginResponseDto
            {
                Success = true,
                Message = "Inicio de sesión exitoso.",
                AccessToken = token,
                AccessTokenExpiresAt = expiresAt,
                RefreshToken = refreshToken,
                Roles = usuario.UsuarioRoles.Select(ur => ur.Rol.Nombre).Distinct().OrderBy(rol => rol).ToList()
            };
               
        }
    }
}