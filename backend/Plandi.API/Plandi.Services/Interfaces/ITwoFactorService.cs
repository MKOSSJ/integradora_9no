using Plandi.Dto.Auth;

namespace Plandi.Services.Interfaces
{
    public interface ITwoFactorService
    {
        public Task<LoginResponseDto> VerifyTwoFactorCodeAsync(TwoFactorDto twoFactorDto);
    }
}