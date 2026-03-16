namespace MusicShop.Service.ViewModels.Cart;

/// <summary>
/// 購物車項目 ViewModel（展示層使用）
/// 包含購物車顯示所需的所有欄位（攤平 CartItem + Album 關聯）
/// </summary>
public class CartItemViewModel
{
    public int Id { get; set; }
    public int AlbumId { get; set; }
    public string AlbumTitle { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int MaxStock { get; set; }
    public decimal SubTotal => Price * Quantity;
    public string FormattedPrice => Price.ToString("N0");
    public string FormattedSubTotal => SubTotal.ToString("N0");
}
