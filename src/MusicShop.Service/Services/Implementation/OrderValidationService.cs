using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels.Cart;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 訂單驗證服務實作
/// 負責訂單相關的業務驗證邏輯，避免重複程式碼
/// </summary>
public class OrderValidationService : IOrderValidationService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderValidationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 驗證購物車項目並準備訂單項目
    /// 此方法整合了購物車驗證、庫存檢查、訂單項目建立的共用邏輯
    /// </summary>
    public async Task<(List<OrderItem> OrderItems, decimal TotalAmount, Dictionary<int, Album> AlbumCache)>
        ValidateAndPrepareOrderItemsAsync(IEnumerable<CartItem> cartItems)
    {
        var cartItemsList = cartItems.ToList();
        ValidationHelper.ValidateCollectionNotEmpty(cartItemsList, "購物車");

        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();
        var albumCache = new Dictionary<int, Album>();

        foreach (var cartItem in cartItemsList)
        {
            // 查詢專輯
            var album = await _unitOfWork.Albums.GetAlbumByIdAsync(cartItem.AlbumId);
            ValidationHelper.ValidateEntityExists(album, "專輯", cartItem.AlbumId);

            // 檢查庫存
            ValidationHelper.ValidateCondition(
                album!.Stock >= cartItem.Quantity,
                $"專輯「{album.Title}」庫存不足，目前庫存: {album.Stock}"
            );

            // 建立訂單項目
            var orderItem = new OrderItem
            {
                AlbumId = cartItem.AlbumId,
                Quantity = cartItem.Quantity,
                UnitPrice = album.Price
            };

            orderItems.Add(orderItem);
            totalAmount += album.Price * cartItem.Quantity;

            // 快取專輯物件，供後續扣除庫存使用
            albumCache[album.Id] = album;
        }

        return (orderItems, totalAmount, albumCache);
    }

    /// <summary>
    /// 驗證結帳資訊（超商門市、發票資訊）
    /// 注意：基本欄位驗證（收件人、地址、門市）已由 CheckoutViewModel.Validate 處理
    /// 這裡只做額外的業務邏輯驗證
    /// </summary>
    public void ValidateCheckoutInfo(CheckoutViewModel checkoutInfo)
    {
        if (checkoutInfo == null)
            throw new ArgumentNullException(nameof(checkoutInfo), "結帳資訊不能為空");

        // 注意：收件人、地址、門市資訊的驗證已由 CheckoutViewModel.Validate 處理
        // 這裡不需要重複驗證，避免邏輯衝突

        // 僅保留發票資訊的額外業務驗證（如果需要的話）
        // CheckoutViewModel.Validate 已經處理了基本的空值檢查
        // 這裡可以加入額外的業務規則驗證（目前沒有額外規則）
    }

    /// <summary>
    /// 扣除訂單項目的庫存
    /// 使用快取避免重複查詢資料庫
    /// </summary>
    public async Task DeductStockAsync(List<OrderItem> orderItems, Dictionary<int, Album> albumCache)
    {
        foreach (var orderItem in orderItems)
        {
            if (albumCache.TryGetValue(orderItem.AlbumId, out var album))
            {
                album.Stock -= orderItem.Quantity;
                await _unitOfWork.Albums.UpdateAlbumAsync(album);
            }
        }
    }
}
