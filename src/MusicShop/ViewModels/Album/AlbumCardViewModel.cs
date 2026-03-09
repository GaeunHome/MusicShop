using AlbumModel = MusicShop.Models.Album;
using MusicShop.Extensions;

namespace MusicShop.ViewModels.Album;

/// <summary>
/// 商品卡片 ViewModel（用於首頁、列表頁、相關商品等）
/// </summary>
public class AlbumCardViewModel
{
    // ==================== 資料屬性 ====================

    /// <summary>
    /// 專輯資料
    /// </summary>
    public AlbumModel Album { get; set; } = null!;

    // ==================== 顯示邏輯屬性 ====================

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int Id => Album.Id;

    /// <summary>
    /// 商品標題
    /// </summary>
    public string Title => Album.Title;

    /// <summary>
    /// 藝人名稱
    /// </summary>
    public string? Artist => Album.Artist?.Name;

    /// <summary>
    /// 封面圖片 URL（取第一張）
    /// </summary>
    public string? CoverImageUrl
    {
        get
        {
            if (string.IsNullOrEmpty(Album.CoverImageUrl))
                return null;

            var urls = Album.CoverImageUrl.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return urls.Length > 0 ? urls[0] : null;
        }
    }

    /// <summary>
    /// 是否有封面圖片
    /// </summary>
    public bool HasCoverImage => !string.IsNullOrEmpty(CoverImageUrl);

    // ==================== 價格相關 ====================

    /// <summary>
    /// 格式化價格（例如：1,250）
    /// </summary>
    public string FormattedPrice => Album.Price.ToTaiwanPrice();

    /// <summary>
    /// 完整價格顯示（例如：NT$ 1,250）
    /// </summary>
    public string FullPrice => Album.Price.ToFullTaiwanPrice();

    // ==================== 庫存相關 ====================

    /// <summary>
    /// 是否有庫存
    /// </summary>
    public bool IsInStock => Album.Stock.IsInStock();

    /// <summary>
    /// 是否庫存不足
    /// </summary>
    public bool IsLowStock => Album.Stock.IsLowStock();

    /// <summary>
    /// 是否已售完
    /// </summary>
    public bool IsSoldOut => Album.Stock.IsSoldOut();

    /// <summary>
    /// 庫存狀態文字
    /// </summary>
    public string StockStatusText => Album.Stock.GetStockStatusText();

    /// <summary>
    /// 庫存狀態 CSS 類別
    /// </summary>
    public string StockStatusCssClass => Album.Stock.GetStockStatusCssClass();

    /// <summary>
    /// 庫存狀態圖示
    /// </summary>
    public string StockStatusIcon => Album.Stock.GetStockStatusIcon();

    // ==================== 分類相關 ====================

    /// <summary>
    /// 藝人分類名稱（透過 Artist 間接取得）
    /// </summary>
    public string? ArtistCategoryName => Album.Artist?.ArtistCategory?.Name;

    /// <summary>
    /// 商品類型名稱
    /// </summary>
    public string? ProductTypeName => Album.ProductType?.Name;

    /// <summary>
    /// 是否有藝人分類（透過 Artist 間接取得）
    /// </summary>
    public bool HasArtistCategory => Album.Artist?.ArtistCategory != null;

    /// <summary>
    /// 是否有商品類型
    /// </summary>
    public bool HasProductType => Album.ProductType != null;
}
