namespace MusicShop.Service.ViewModels.Wishlist;

/// <summary>
/// 收藏清單項目 ViewModel（展示層使用）
/// 包含收藏清單顯示所需的所有欄位（攤平 WishlistItem + Album 關聯）
/// </summary>
public class WishlistItemViewModel
{
    public int Id { get; set; }
    public int AlbumId { get; set; }
    public string AlbumTitle { get; set; } = string.Empty;
    public string? ArtistName { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime AddedAt { get; set; }

    public bool IsInStock => Stock > 0;
    public bool IsSoldOut => Stock <= 0;
    public bool IsLowStock => Stock > 0 && Stock <= 5;
    public string FormattedPrice => $"NT${Price.ToString("N0")}";
    public string StockStatusText => Stock <= 0 ? "已售完" : Stock <= 5 ? $"僅剩 {Stock} 件" : "有庫存";
}
