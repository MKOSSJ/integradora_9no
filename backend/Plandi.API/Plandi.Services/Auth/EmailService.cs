using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Plandi.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task SendPasswordResetEmailAsync(Usuario usuario, string resetToken)
        {
            var frontendUrl = _configuration["App:FrontendUrl"]?.TrimEnd('/');
            string body;

            if (!string.IsNullOrWhiteSpace(frontendUrl))
            {
                var resetUrl = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";
                body = $"Hola {usuario.Nombre},\n\nPara restablecer tu contraseña, abre el siguiente enlace:\n{resetUrl}\n\nSi no solicitaste este cambio, ignora este mensaje.";
            }
            else
            {
                body = $"Hola {usuario.Nombre},\n\nUsa este token para restablecer tu contraseña:\n{resetToken}\n\nSi no solicitaste este cambio, ignora este mensaje.";
            }

            _logger.LogInformation("Password reset email prepared for {Email}: {Body}", usuario.Email, body);
            return Task.CompletedTask;
        }
    }
}
