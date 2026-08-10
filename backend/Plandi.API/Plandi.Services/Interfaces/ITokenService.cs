using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Auth;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.Services.Interfaces

{
    public interface ITokenService
    {   
    Task<(string Token, DateTime expiresAt)> GenerateAccessToken(Usuario usuario);
    Task<(string RefreshToken, string TokenHash, DateTime ExpiresAt)> GenerateRefreshToken();

    Task<RequestTokenResponse> GenerateNewAccessToken (RequestToken requestToken);
    }   
}