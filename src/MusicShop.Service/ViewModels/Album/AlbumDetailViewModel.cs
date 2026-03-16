using MusicShop.Library.Extensions;

namespace MusicShop.Service.ViewModels.Album;

/// <summary>
/// 商品詳情頁面 ViewModel（包含所有顯示邏輯）
/// 所有欄位已攤平，不含 Data 層實體參考
/// </summary>
public class AlbumDetailViewModel
{
    // ==================== 資料屬性 ====================

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DescriptionImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? ArtistName { get; set; }
    public int? ArtistId { get; set; }
    public int? ArtistCategoryId { get; set; }
    public int? ProductTypeId { get; set; }

    /// <summary>
    /// 相關商品推薦（同藝人分類或同商品類型）
    /// </summary>
    public List<AlbumCardViewModel> RelatedAlbums { get; set; } = new List<AlbumCardViewModel>();

    /// <summary>
    /// 預設數量
    /// </summary>
    public int DefaultQuantity { get; set; } = 1;

    /// <summary>
    /// 是否為預購商品
    /// </summary>
    public bool IsPreOrder { get; set; } = false;

    /// <summary>
    /// 韓國出貨日期
    /// </summary>
    public DateTime? KoreanShippingDate { get; set; }

    // ==================== 顯示邏輯屬性（View 使用） ====================

    /// <summary>
    /// 商品編號（格式化）
    /// </summary>
    public string ProductCode => $"ALBUM{Id:D6}";

    /// <summary>
    /// 商品圖片清單（處理後的 URL 列表）
    /// </summary>
    public List<string> ImageUrls { get; set; } = new List<string>();

    /// <summary>
    /// 是否有圖片
    /// </summary>
    public bool HasImages => ImageUrls.Any();

    /// <summary>
    /// 是否有多張圖片
    /// </summary>
    public bool HasMultipleImages => ImageUrls.Count > 1;

    /// <summary>
    /// 第一張圖片 URL
    /// </summary>
    public string? FirstImageUrl => ImageUrls.FirstOrDefault();

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

    // ==================== 預購相關 ====================

    /// <summary>
    /// 是否顯示預購提示
    /// </summary>
    public bool ShowPreOrderNotice => IsPreOrder && KoreanShippingDate.HasValue;

    /// <summary>
    /// 格式化韓國出貨日期
    /// </summary>
    public string? FormattedKoreanShippingDate => KoreanShippingDate?.ToKoreanShippingDate();

    // ==================== 商品介紹相關 ====================

    /// <summary>
    /// 是否有商品介紹圖片
    /// </summary>
    public bool HasDescriptionImage => !string.IsNullOrEmpty(DescriptionImageUrl);

    /// <summary>
    /// 是否有文字說明
    /// </summary>
    public bool HasDescription => !string.IsNullOrEmpty(Description);

    /// <summary>
    /// 是否有任何商品介紹
    /// </summary>
    public bool HasAnyDescription => HasDescriptionImage || HasDescription;

    // ==================== 相關商品 ====================

    /// <summary>
    /// 是否有相關商品
    /// </summary>
    public bool HasRelatedAlbums => RelatedAlbums.Any();
}
