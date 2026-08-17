using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Plandi.Services.Interfaces;
using Plandi.Dto.Auth;
using Plandi.API.Security;

namespace Plandi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly ILogger<AuthController> _logger;
        private readonly PasswordRecoveryRateLimiter _passwordRecoveryRateLimiter;

        public AuthController(IAuthService authService, ITwoFactorService twoFactorService, ILogger<AuthController> logger, PasswordRecoveryRateLimiter passwordRecoveryRateLimiter)
        {
            _authService = authService;
            _twoFactorService = twoFactorService;
            _logger = logger;
            _passwordRecoveryRateLimiter = passwordRecoveryRateLimiter;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (registerDto == null)
            {
                return BadRequest(new { message = "Datos de registro no proporcionados." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.RegisterUser(registerDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new RegisterResponseDto { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user.");
                return StatusCode(StatusCodes.Status500InternalServerError, new RegisterResponseDto
                {
                    Success = false,
                    Message = "Ocurrió un error interno en el servidor."
                });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("loginPolicy")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest(new { message = "Datos de inicio de sesión no proporcionados." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.LoginUser(loginDto);
                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new LoginResponseDto { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging in user.");
                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponseDto
                {
                    Success = false,
                    Message = "Ocurrió un error interno en el servidor."
                });
            }
        }


        [HttpPost("two-factor-verify")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RequestTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyTwoFactorCode([FromBody] TwoFactorDto twoFactorDto, [FromServices] ITwoFactorService twoFactorService)
        {
            if (twoFactorDto == null)
            {
                return BadRequest(new { message = "Datos de verificación no proporcionados." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await twoFactorService.VerifyTwoFactorCodeAsync(twoFactorDto);
                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying two-factor code.");
                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponseDto
                {
                    Success = false,
                    Message = "Ocurrió un error interno en el servidor."
                });
            }
        }


        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RequestTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RequestToken requestToken)
        {
            if (requestToken == null)
            {
                return BadRequest(new { message = "Refresh token no proporcionado." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.RefreshTokenAsync(requestToken);
                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing token.");
                return StatusCode(StatusCodes.Status500InternalServerError, new RequestTokenResponse
                {
                    Success = false,
                    Message = "Ocurrió un error interno en el servidor."
                });
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (forgotPasswordDto == null)
            {
                return BadRequest(new { message = "Datos de recuperación no proporcionados." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_passwordRecoveryRateLimiter.TryAcquire(RemoteIp, forgotPasswordDto.Email))
                return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Demasiadas solicitudes. Intente más tarde." });

            try
            {
                await _authService.ForgotPasswordAsync(forgotPasswordDto);
                return Ok(new { message = "Si el correo existe, se ha enviado un enlace de recuperación." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during forgot password.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error interno en el servidor." });
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto == null)
            {
                return BadRequest(new { message = "Datos de restablecimiento no proporcionados." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_passwordRecoveryRateLimiter.TryAcquire(RemoteIp, resetPasswordDto.PasswordResetToken))
                return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Demasiados intentos. Intente más tarde." });

            try
            {
                var result = await _authService.ResetPasswordAsync(resetPasswordDto);
                if (!result)
                {
                    return Unauthorized(new { message = "Token inválido o expirado." });
                }

                return Ok(new { message = "Contraseña restablecida correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during reset password.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error interno en el servidor." });
            }
        }

        private string RemoteIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
