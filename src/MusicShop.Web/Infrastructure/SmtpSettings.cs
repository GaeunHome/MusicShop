namespace MusicShop.Web.Infrastructure;

/// <summary>
/// SMTP 郵件伺服器設定模型
/// 對應 appsettings.json 的 SmtpSettings 區段
/// </summary>
public class SmtpSettings
{
    /// <summary>
    /// SMTP 伺服器位址（例如 smtp.gmail.com）
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP 連接埠（通常 587 for TLS, 465 for SSL）
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// 是否啟用 SSL/TLS
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// 寄件人 Email
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// 寄件人顯示名稱
    /// </summary>
    public string FromName { get; set; } = "MusicShop";

    /// <summary>
    /// SMTP 認證帳號
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// SMTP 認證密碼（或應用程式密碼）
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
