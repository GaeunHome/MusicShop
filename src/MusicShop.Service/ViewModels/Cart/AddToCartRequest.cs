namespace MusicShop.Service.ViewModels.Cart;

/// <summary>
/// 加入購物車的 API 請求模型
/// </summary>
public class AddToCartRequest
{
    public int AlbumId { get; set; }
    public int Quantity { get; set; } = 1;
}
