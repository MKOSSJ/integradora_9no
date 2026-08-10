using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Plandi.Dto.Auth;
using Microsoft.EntityFrameworkCore;

namespace Plandi.Services
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _dBContext;
        private readonly IConfiguration _config;

        public TokenService(AppDbContext dBContext, IConfiguration config)
        {
            _dBContext = dBContext;
            _config = config;
        }

        public async Task<(string Token, DateTime expiresAt)> GenerateAccessToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, usuario.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
            };

            claims.AddRange(usuario.UsuarioRoles.Select(ur =>
                new Claim(ClaimTypes.Role, ur.Rol.Nombre)));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationMinutes = int.Parse(_config["Jwt:AccessTokenExpirationMinutes"]!);

            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);  

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        public async Task<(string RefreshToken, string TokenHash, DateTime ExpiresAt)> GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            
            string refreshToken = Convert.ToBase64String(randomBytes);

            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            string tokenHash = Convert.ToBase64String(hashBytes);

            int days = int.Parse(_config["Jwt:RefreshTokenExpirationDays"]!);
            DateTime expiresAt = DateTime.UtcNow.AddDays(days);

            return (refreshToken, tokenHash, expiresAt);
        }

        public async Task<RequestTokenResponse> GenerateNewAccessToken (RequestToken requestToken)
        {
            
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(requestToken.RefreshToken));
            string tokenHashCalculado = Convert.ToBase64String(hashBytes);

            var tokenExist = await _dBContext.RefreshTokens
                .Include(rt => rt.Usuario)
                    .ThenInclude(u => u.UsuarioRoles)
                        .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHashCalculado);

            if (tokenExist == null || !tokenExist.isActive)
            {
                return new RequestTokenResponse
                {
                    Success = false,
                    Message = "El token no es válido o ha expirado."
                };
            }

            var usuario = tokenExist.Usuario;
            var (newAccessToken, newExpiresAt) = await GenerateAccessToken(usuario);
            var (newRefreshToken, newTokenHash, newRefreshTokenExpiresAt) = await GenerateRefreshToken();

            tokenExist.Revoked = DateTime.UtcNow;
            tokenExist.ReplacedByTokenHash = newTokenHash;

            var newRefreshTokenEntity = new RefreshToken
            {
                UsuarioId = usuario.Id,
                TokenHash = newTokenHash,
                Expires = newRefreshTokenExpiresAt,
                Created = DateTime.UtcNow
            };

            _dBContext.RefreshTokens.Add(newRefreshTokenEntity);
            await _dBContext.SaveChangesAsync();
            
            return new RequestTokenResponse
            {
                Success = true,
                Message = "Nuevo token generado exitosamente.",
                AccessToken = newAccessToken,
                AccessTokenExpiresAt = newExpiresAt,    
                RefreshToken = newRefreshToken
            };
        }
    }
}