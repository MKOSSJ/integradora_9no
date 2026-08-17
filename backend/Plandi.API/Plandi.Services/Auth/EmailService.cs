using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
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

        public async Task SendPasswordResetEmailAsync(Usuario usuario, string resetToken)
        {
            var frontendUrl = _configuration["App:FrontendUrl"]?.TrimEnd('/');
            string resetUrl = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

            var email = new MimeMessage();
            var senderName = _configuration["EmailSettings:SenderName"] ?? "Plandi";
            var senderEmail = _configuration["EmailSettings:From"] 
                ?? throw new InvalidOperationException("EmailSettings:From no está configurado.");
            
            var recipientEmail = usuario.Email 
                ?? throw new InvalidOperationException("El usuario no tiene un correo electrónico válido.");

            email.From.Add(new MailboxAddress(senderName, senderEmail));
            email.To.Add(new MailboxAddress(usuario.Nombre, recipientEmail));
            
            email.Subject = "Restablece tu contraseña - Plandi";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2>Hola, {usuario.Nombre}</h2>
                        <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en <strong>Plandi</strong>.</p>
                        <p>Haz clic en el siguiente botón para crear una nueva contraseña:</p>
                        <p style='margin: 20px 0;'>
                            <a href='{resetUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                                Restablecer Contraseña
                            </a>
                        </p>
                        <p>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                        <p><a href='{resetUrl}'>{resetUrl}</a></p>
                        <hr style='border: none; border-top: 1px solid #ccc; margin-top: 30px;' />
                        <p style='font-size: 12px; color: #777;'>Si no solicitaste este cambio, puedes ignorar este mensaje de forma segura.</p>
                    </div>"
            };

            email.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var smtp = new SmtpClient();
                
                var host = _configuration["EmailSettings:SmtpServer"] 
                    ?? throw new InvalidOperationException("EmailSettings:SmtpServer no está configurado.");
                var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                var username = _configuration["EmailSettings:Username"] 
                    ?? throw new InvalidOperationException("EmailSettings:Username no está configurado.");
                var password = _configuration["EmailSettings:Password"] 
                    ?? throw new InvalidOperationException("EmailSettings:Password no está configurado.");

                await smtp.AuthenticateAsync(username, password);

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Correo de recuperación enviado exitosamente a {Email}", recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el correo de recuperación a {Email}", usuario.Email);
                throw;
            }
        }
    
        public async  Task SendTwoFactorCodeEmailAsync(Usuario usuario, string twoFactorCode)
        {
            var email = new MimeMessage();
            var senderName = _configuration["EmailSettings:SenderName"] ?? "Plandi";
            var senderEmail = _configuration["EmailSettings:From"] 
                ?? throw new InvalidOperationException("EmailSettings:From no está configurado.");
            
            var recipientEmail = usuario.Email 
                ?? throw new InvalidOperationException("El usuario no tiene un correo electrónico válido.");

            email.From.Add(new MailboxAddress(senderName, senderEmail));
            email.To.Add(new MailboxAddress(usuario.Nombre, recipientEmail));
            email.Subject = "Código de Autenticación de Dos Factores";
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; max-width: 500px;'>
                    <h2>Hola, {usuario.Nombre}</h2>
                    <p>Tu código de verificación para iniciar sesión en <strong>Plandi</strong> es:</p>
                    <div style='background-color: #f4f4f4; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; border-radius: 5px; margin: 20px 0;'>
                        {twoFactorCode}
                    </div>
                    <p>Este código expira en 5 minutos.</p>
                    <hr style='border: none; border-top: 1px solid #ccc; margin-top: 30px;' />
                    <p style='font-size: 12px; color: #777;'>Si no intentaste iniciar sesión, ignora este mensaje o cambia tu contraseña.</p>
                </div>"
            };
            email.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var smtp = new SmtpClient();
                
                var host = _configuration["EmailSettings:SmtpServer"] 
                    ?? throw new InvalidOperationException("EmailSettings:SmtpServer no está configurado.");
                var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                var username = _configuration["EmailSettings:Username"] 
                    ?? throw new InvalidOperationException("EmailSettings:Username no está configurado.");
                var password = _configuration["EmailSettings:Password"] 
                    ?? throw new InvalidOperationException("EmailSettings:Password no está configurado.");

                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Código 2FA enviado a {Email}", recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar código 2FA a {Email}", usuario.Email);
                throw;
            }
        }         
    }
}