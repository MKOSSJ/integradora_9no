using System.Threading.Tasks;
using Plandi.Library.Models;

namespace Plandi.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(Usuario usuario, string resetToken);
        Task SendTwoFactorCodeEmailAsync(Usuario usuario, string twoFactorCode);
    }
}
