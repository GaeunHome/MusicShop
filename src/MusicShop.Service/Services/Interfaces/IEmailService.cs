namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// Email 寄送服務介面
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// 寄送 Email
    /// </summary>
    /// <param name="toEmail">收件人 Email</param>
    /// <param name="subject">主旨</param>
    /// <param name="htmlBody">HTML 內容</param>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
