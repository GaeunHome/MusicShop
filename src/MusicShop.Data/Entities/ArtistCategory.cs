using System.ComponentModel.DataAnnotations;

namespace MusicShop.Data.Entities;

/// <summary>
/// 藝人分類模型（男子團體、女子團體等）
/// </summary>
public class ArtistCategory
{
    /// <summary>
    /// 藝人分類 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 藝人分類名稱（例如：男子團體、女子團體、獨立藝人）
    /// </summary>
    [Required(ErrorMessage = "藝人分類名稱為必填")]
    [StringLength(50, ErrorMessage = "藝人分類名稱不可超過 50 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(200, ErrorMessage = "描述不可超過 200 字元")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序順序（用於前端顯示排序）
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// 此藝人分類下的所有商品
    /// </summary>
    public ICollection<Album> Albums { get; set; } = new List<Album>();

    /// <summary>
    /// 此藝人分類下的所有藝人/團體（如：BOY GROUP 分類下的 2PM、BTS、ASTRO 等）
    /// </summary>
    public ICollection<Artist> Artists { get; set; } = new List<Artist>();
}
