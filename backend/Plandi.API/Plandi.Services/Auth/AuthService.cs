using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Auth;
using Plandi.Library.Models;
using Plandi.Dto.Utils;
using Plandi.Services.Interfaces;
using System.Data;
using System.Security.Cryptography;

namespace Plandi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dBContext;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext dbContext, ITokenService tokenService, IEmailService emailService)
        {
            _dBContext = dbContext;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<RegisterResponseDto> RegisterUser(RegisterDTO registerDto)
        {
            string emailCleaned = registerDto.Email.Trim().ToLower();

            bool existingUser = await _dBContext.Usuarios.AnyAsync(u => u.Email != null && u.Email.ToLower() == emailCleaned);
            if (existingUser)
            {
                throw new InvalidOperationException("El correo electrónico ya está registrado.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var newUsuario = new Usuario
            {
                Nombre = registerDto.Nombre.Trim(),
                ApellidoPaterno = registerDto.ApellidoPaterno.Trim(),
                ApellidoMaterno = registerDto.ApellidoMaterno?.Trim(),
                Email = emailCleaned,
                PasswordHash = passwordHash,
                Telefono = registerDto.Telefono?.Trim()
            };
            var rolDocente = await _dBContext.Roles.SingleAsync(rol => rol.Nombre == "Docente");
            var usuarioRol = new UsuarioRol { Usuario = newUsuario, RolId = rolDocente.Id };

            _dBContext.Usuarios.Add(newUsuario);
            _dBContext.UsuarioRoles.Add(usuarioRol);
            await _dBContext.SaveChangesAsync();

            return new RegisterResponseDto
            {
                Success = true,
                Message = "Usuario registrado.",
            };
        }

        public async Task<LoginResponseDto> LoginUser(LoginDTO loginDto)
        {
            string emailCleaned = loginDto.Email.Trim().ToLower();

            var usuario = await _dBContext.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == emailCleaned && u.Activo && u.DeletedAt == null);

            if (usuario == null)
            {
                throw new InvalidOperationException("Usuario o contraseña incorrectos.");
            }

            if (usuario.LockoutEnd.HasValue && usuario.LockoutEnd.Value > DateTime.UtcNow)
            {
                var minutosRestantes = Math.Ceiling((usuario.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Cuenta bloqueada temporalmente. Intente nuevamente en {minutosRestantes} minuto(s)."
                };
            }

            bool isPasswordValid = usuario.PasswordHash is not null && BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.PasswordHash);

            if (!isPasswordValid)
            {
                usuario.AccessFailedCount++;

                if (usuario.AccessFailedCount >= 5)
                {
                    usuario.LockoutEnd = DateTime.UtcNow.AddMinutes(5);
                    await _dBContext.SaveChangesAsync();

                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Cuenta bloqueada temporalmente debido a múltiples intentos fallidos. Espere 5 minutos."
                    };
                }

                await _dBContext.SaveChangesAsync();

                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Usuario o contraseña incorrectos."
                };
            }

            usuario.AccessFailedCount = 0;
            usuario.LockoutEnd = null;
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
                RequiresTwoFactor = usuario.TwoFactorEnabled,
                Roles = usuario.UsuarioRoles.Select(ur => ur.Rol.Nombre).Distinct().OrderBy(rol => rol).ToList()
            };
        }

        public async Task<RequestTokenResponse> RefreshTokenAsync(RequestToken requestToken)
        {
            return await _tokenService.GenerateNewAccessToken(requestToken);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var emailCleaned = forgotPasswordDto.Email.Trim().ToLower();
            var usuario = await _dBContext.Usuarios.FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == emailCleaned);

            if (usuario == null)
            {
                return;
            }

            var resetToken = Utils.GenerateCode();
            usuario.PasswordResetToken = Utils.HashToken(resetToken);
            usuario.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _dBContext.SaveChangesAsync();
            await _emailService.SendPasswordResetEmailAsync(usuario, resetToken);
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var token = resetPasswordDto.PasswordResetToken?.Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var tokenHash = Utils.HashToken(token);
            var usuario = await _dBContext.Usuarios.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == tokenHash && u.Activo && u.DeletedAt == null &&
                u.PasswordResetTokenExpires.HasValue &&
                u.PasswordResetTokenExpires.Value > DateTime.UtcNow);

            if (usuario == null)
            {
                return false;
            }

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            usuario.PasswordResetToken = null;
            usuario.PasswordResetTokenExpires = null;

            await _dBContext.SaveChangesAsync();
            return true;
        }
    }
}
