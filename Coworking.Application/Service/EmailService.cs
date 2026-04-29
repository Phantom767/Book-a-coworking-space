using Coworking.Application.Interfaces;
using Coworking.Application.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Coworking.Application.Service
{
    public class EmailSender(IOptions<EmailSettings> emailSettings) : IEmailSender
    {
        private readonly EmailSettings _emailSettings = emailSettings.Value;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    // Используем Auto, чтобы MailKit сам определил SSL/TLS
                    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.Auto);
                    
                    // Аутентификация
                    await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                    
                    // Отправка
                    await client.SendAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    // Логируйте ошибку здесь!
                    Console.WriteLine($"Ошибка отправки письма: {ex.Message}");
                    throw; // Пробрасываем ошибку дальше, чтобы контроллер знал о неудаче
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}