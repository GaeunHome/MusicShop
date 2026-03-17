using MusicShop.Library.Helpers;

namespace MusicShop.Service.ViewModels.Album;

/// <summary>
/// 商品卡片 ViewModel（用於首頁、列表頁、相關商品等）
/// 所有欄位已攤平，不含 Data 層實體參考
/// </summary>
public class AlbumCardViewModel
{
    // ==================== 資料屬性 ====================

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? ArtistName { get; set; }
    public int? ArtistId { get; set; }
    public string? ArtistCategoryName { get; set; }
    public string? ProductTypeName { get; set; }

    // ==================== 顯示邏輯屬性 ====================

    /// <summary>
    /// 是否有封面圖片
    /// </summary>
    public bool HasCoverImage => !string.IsNullOrEmpty(CoverImageUrl);

    // ==================== 價格相關 ====================

    /// <summary>
    /// 格式化價格（例如：1,250）
    /// </summary>
    public string FormattedPrice => Price.ToTaiwanPrice();

    /// <summary>
    /// 完整價格顯示（例如：NT$ 1,250）
    /// </summary>
    public string FullPrice => Price.ToFullTaiwanPrice();

    // ==================== 庫存相關 ====================

    /// <summary>
    /// 是否有庫存
    /// </summary>
    public bool IsInStock => Stock.IsInStock();

    /// <summary>
    /// 是否庫存不足
    /// </summary>
    public bool IsLowStock => Stock.IsLowStock();

    /// <summary>
    /// 是否已售完
    /// </summary>
    public bool IsSoldOut => Stock.IsSoldOut();

    /// <summary>
    /// 庫存狀態文字
    /// </summary>
    public string StockStatusText => Stock.GetStockStatusText();

    /// <summary>
    /// 庫存狀態 CSS 類別
    /// </summary>
    public string StockStatusCssClass => Stock.GetStockStatusCssClass();

    /// <summary>
    /// 庫存狀態圖示
    /// </summary>
    public string StockStatusIcon => Stock.GetStockStatusIcon();

    // ==================== 分類相關 ====================

    /// <summary>
    /// 是否有藝人分類
    /// </summary>
    public bool HasArtistCategory => !string.IsNullOrEmpty(ArtistCategoryName);

    /// <summary>
    /// 是否有商品類型
    /// </summary>
    public bool HasProductType => !string.IsNullOrEmpty(ProductTypeName);
}
