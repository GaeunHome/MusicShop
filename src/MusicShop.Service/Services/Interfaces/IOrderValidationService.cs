using MusicShop.Data.Entities;
using MusicShop.Service.ViewModels.Cart;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 訂單驗證服務介面
/// 負責訂單相關的業務驗證邏輯
/// </summary>
public interface IOrderValidationService
{
    /// <summary>
    /// 驗證購物車項目並準備訂單項目
    /// </summary>
    /// <param name="cartItems">購物車項目清單</param>
    /// <returns>(訂單項目清單, 總金額, 專輯快取)</returns>
    Task<(List<OrderItem> OrderItems, decimal TotalAmount, Dictionary<int, Album> AlbumCache)>
        ValidateAndPrepareOrderItemsAsync(IEnumerable<CartItem> cartItems);

    /// <summary>
    /// 驗證結帳資訊（超商門市、發票資訊）
    /// </summary>
    /// <param name="checkoutInfo">結帳資訊</param>
    void ValidateCheckoutInfo(CheckoutViewModel checkoutInfo);

    /// <summary>
    /// 扣除訂單項目的庫存
    /// </summary>
    /// <param name="orderItems">訂單項目清單</param>
    /// <param name="albumCache">專輯快取（避免重複查詢）</param>
    Task DeductStockAsync(List<OrderItem> orderItems, Dictionary<int, Album> albumCache);
}
