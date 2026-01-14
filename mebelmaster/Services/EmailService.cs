using MailKit.Net.Smtp;
using MimeKit;
using MebelMaster.Models;
using Microsoft.Extensions.Options;

namespace MebelMaster.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendOrderNotificationAsync(Order order)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("Manager", "manager@mebelmaster.ru"));
                message.Subject = $"Новая заявка от {order.CustomerName}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <h2>Новая заявка с сайта</h2>
                        <p><strong>Имя:</strong> {order.CustomerName}</p>
                        <p><strong>Телефон:</strong> {order.Phone}</p>
                        <p><strong>Email:</strong> {order.Email ?? "Не указан"}</p>
                        <p><strong>Сообщение:</strong> {order.Message ?? "Не указано"}</p>
                        {(order.ProductId.HasValue ? $"<p><strong>Товар:</strong> {order.Product?.Name}</p>" : "")}
                        <p><strong>Дата:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}</p>
                    "
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, false);
                await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email notification sent for order {OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email notification for order {OrderId}", order.Id);
                // Не бросаем исключение, чтобы не прерывать работу сайта
            }
        }

        public async Task SendContactFormAsync(string name, string email, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                emailMessage.To.Add(new MailboxAddress("Manager", "manager@mebelmaster.ru"));
                emailMessage.Subject = "Новое сообщение с формы обратной связи";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <h2>Новое сообщение с сайта</h2>
                        <p><strong>Имя:</strong> {name}</p>
                        <p><strong>Email:</strong> {email}</p>
                        <p><strong>Сообщение:</strong> {message}</p>
                        <p><strong>Дата:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                    "
                };

                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, false);
                await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Contact form email sent from {Name}", name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contact form email from {Name}", name);
            }
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}