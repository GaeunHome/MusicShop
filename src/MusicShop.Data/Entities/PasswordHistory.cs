namespace MusicShop.Data.Entities;

/// <summary>
/// 密碼歷史記錄實體
/// 儲存使用者過去使用過的密碼雜湊值，防止重複使用舊密碼
/// </summary>
public class PasswordHistory
{
    public int Id { get; set; }

    /// <summary>
    /// 使用者 ID（FK → AspNetUsers）
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 密碼雜湊值（由 Identity PasswordHasher 產生）
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 密碼設定時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 導覽屬性
    public AppUser User { get; set; } = null!;
}
