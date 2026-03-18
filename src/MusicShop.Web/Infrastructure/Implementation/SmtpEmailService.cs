using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Web.Infrastructure.Implementation;

/// <summary>
/// SMTP Email 寄送服務實作
/// 使用 System.Net.Mail 透過 SMTP 伺服器寄送郵件
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(_settings.Host, _settings.Port);
            client.EnableSsl = _settings.EnableSsl;
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

            await client.SendMailAsync(message);

            _logger.LogInformation("Email 寄送成功：To={ToEmail}, Subject={Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email 寄送失敗：To={ToEmail}, Subject={Subject}", toEmail, subject);
            throw;
        }
    }
}
