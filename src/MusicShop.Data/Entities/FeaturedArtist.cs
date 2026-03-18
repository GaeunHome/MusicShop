namespace MusicShop.Data.Entities;

/// <summary>
/// 首頁精選藝人（管理員可設定哪些藝人出現在首頁精選特區）
/// </summary>
public class FeaturedArtist : ISoftDeletable
{
    public int Id { get; set; }

    /// <summary>
    /// 關聯的藝人 ID（同一藝人可被多次精選）
    /// </summary>
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    /// <summary>
    /// 顯示順序（數字越小越前面）
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ===== 軟刪除欄位 =====
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
