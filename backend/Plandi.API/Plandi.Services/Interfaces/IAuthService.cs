using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Plandi.Dto.Auth;

namespace Plandi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterUser(RegisterDTO registerDto);
        Task<LoginResponseDto> LoginUser(LoginDTO loginDto);
        Task<RequestTokenResponse> RefreshTokenAsync(RequestToken requestToken);
        Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}
