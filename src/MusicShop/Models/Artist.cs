using System.ComponentModel.DataAnnotations;

namespace MusicShop.Models;

/// <summary>
/// 藝人/團體實體模型（如：2PM、BTS、BLACKPINK 等具體藝人團體）
/// </summary>
public class Artist
{
    /// <summary>
    /// 藝人 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 藝人名稱（如：2PM、BTS、BLACKPINK）
    /// </summary>
    [Required(ErrorMessage = "藝人名稱為必填")]
    [StringLength(100, ErrorMessage = "藝人名稱不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 藝人描述或簡介
    /// </summary>
    [StringLength(500, ErrorMessage = "描述不可超過 500 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 藝人形象圖片 URL
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// 排序順序（用於前端顯示排序）
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    // ===== 外鍵關聯 =====

    /// <summary>
    /// 隸屬於哪個藝人分類（BOY GROUP、GIRL GROUP、SOLO）
    /// </summary>
    public int ArtistCategoryId { get; set; }

    /// <summary>
    /// 導航屬性：隸屬的藝人分類
    /// </summary>
    public ArtistCategory ArtistCategory { get; set; } = null!;

    // ===== 導航屬性 =====

    /// <summary>
    /// 此藝人的所有專輯
    /// </summary>
    public ICollection<Album> Albums { get; set; } = new List<Album>();
}
