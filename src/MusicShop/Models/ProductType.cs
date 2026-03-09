using System.ComponentModel.DataAnnotations;

namespace MusicShop.Models;

/// <summary>
/// 商品類型模型（專輯、手燈、周邊商品等）
/// </summary>
public class ProductType
{
    /// <summary>
    /// 商品類型 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品類型名稱（例如：專輯、手燈、周邊商品、寫真集）
    /// </summary>
    [Required(ErrorMessage = "商品類型名稱為必填")]
    [StringLength(50, ErrorMessage = "商品類型名稱不可超過 50 字元")]
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
    /// 父分類 ID（null 表示為頂層分類，如 K-ALBUM、K-MAGAZINE 等）
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 父分類導航屬性
    /// </summary>
    public ProductType? Parent { get; set; }

    /// <summary>
    /// 子分類集合
    /// </summary>
    public ICollection<ProductType> Children { get; set; } = new List<ProductType>();

    /// <summary>
    /// 此商品類型下的所有商品
    /// </summary>
    public ICollection<Album> Albums { get; set; } = new List<Album>();
}
