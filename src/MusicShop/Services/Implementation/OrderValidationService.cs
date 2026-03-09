using MusicShop.Models;
using MusicShop.Repositories.Interface;
using MusicShop.Services.Interface;
using MusicShop.Helpers;
using MusicShop.ViewModels.Cart;

namespace MusicShop.Services.Implementation;

/// <summary>
/// 訂單驗證服務實作
/// 負責訂單相關的業務驗證邏輯，避免重複程式碼
/// </summary>
public class OrderValidationService : IOrderValidationService
{
    private readonly IAlbumRepository _albumRepository;

    public OrderValidationService(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository;
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
            var album = await _albumRepository.GetAlbumByIdAsync(cartItem.AlbumId);
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
    /// </summary>
    public void ValidateCheckoutInfo(CheckoutViewModel checkoutInfo)
    {
        if (checkoutInfo == null)
            throw new ArgumentNullException(nameof(checkoutInfo), "結帳資訊不能為空");

        // 驗證收件人資訊
        ValidationHelper.ValidateString(checkoutInfo.ReceiverName, "收件人姓名", 100, nameof(checkoutInfo.ReceiverName));
        ValidationHelper.ValidateString(checkoutInfo.ReceiverPhone, "收件人電話", 20, nameof(checkoutInfo.ReceiverPhone));

        // 驗證地址資訊
        ValidationHelper.ValidateString(checkoutInfo.City, "縣市", 50, nameof(checkoutInfo.City));
        ValidationHelper.ValidateString(checkoutInfo.District, "鄉鎮市區", 50, nameof(checkoutInfo.District));
        ValidationHelper.ValidateString(checkoutInfo.PostalCode, "郵遞區號", 10, nameof(checkoutInfo.PostalCode));
        ValidationHelper.ValidateString(checkoutInfo.Address, "詳細地址", 500, nameof(checkoutInfo.Address));

        // 驗證超商門市資訊（如果選擇超商取貨）
        if (!checkoutInfo.IsStoreInfoValid())
            throw new InvalidOperationException("選擇超商取貨時，必須填寫完整的門市資訊");

        // 驗證發票資訊
        if (!checkoutInfo.IsTriplicateInvoiceValid())
            throw new InvalidOperationException("選擇三聯式發票時，必須填寫統一編號和公司抬頭");

        if (!checkoutInfo.IsEInvoiceValid())
            throw new InvalidOperationException("選擇電子發票時，必須填寫載具號碼");
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
                await _albumRepository.UpdateAlbumAsync(album);
            }
        }
    }
}
