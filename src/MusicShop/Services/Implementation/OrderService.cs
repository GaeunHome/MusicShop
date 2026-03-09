using MusicShop.Models;
using MusicShop.Repositories.Interface;
using MusicShop.Services.Interface;
using MusicShop.Helpers;
using MusicShop.ViewModels.Cart;

namespace MusicShop.Services.Implementation
{
    /// <summary>
    /// 訂單商業邏輯實作
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly IOrderValidationService _orderValidationService;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IAlbumRepository albumRepository,
            IOrderValidationService orderValidationService)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _albumRepository = albumRepository;
            _orderValidationService = orderValidationService;
        }

        [Obsolete("請使用 CreateOrderWithFullInfoAsync 方法")]
        public async Task<Order> CreateOrderFromCartAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            // 取得購物車項目
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId);

            // 使用驗證服務準備訂單項目（避免重複程式碼）
            var (orderItems, totalAmount, albumCache) = await _orderValidationService
                .ValidateAndPrepareOrderItemsAsync(cartItems);

            // 建立訂單
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                OrderItems = orderItems
            };

            // 儲存訂單
            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            // 扣除庫存（使用驗證服務統一處理）
            await _orderValidationService.DeductStockAsync(orderItems, albumCache);

            // 清空購物車
            await _cartRepository.ClearCartAsync(userId);

            return createdOrder;
        }

        public async Task<Order> CreateOrderWithFullInfoAsync(string userId, CheckoutViewModel checkoutInfo)
        {
            // ==================== 參數驗證 ====================
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            // 使用驗證服務驗證結帳資訊（避免重複程式碼）
            _orderValidationService.ValidateCheckoutInfo(checkoutInfo);

            // ==================== 取得購物車項目 ====================
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId);

            // ==================== 驗證庫存並計算總金額 ====================
            // 使用驗證服務準備訂單項目（避免重複程式碼）
            var (orderItems, totalAmount, albumCache) = await _orderValidationService
                .ValidateAndPrepareOrderItemsAsync(cartItems);

            // ==================== 根據配送方式選擇門市資料 ====================
            string? storeCode = null;
            string? storeName = null;
            string? storeAddress = null;

            if (checkoutInfo.DeliveryMethod == DeliveryMethod.SevenEleven)
            {
                storeCode = checkoutInfo.SevenElevenStoreCode;
                storeName = checkoutInfo.SevenElevenStoreName;
                storeAddress = checkoutInfo.SevenElevenStoreAddress;
            }
            else if (checkoutInfo.DeliveryMethod == DeliveryMethod.FamilyMart)
            {
                storeCode = checkoutInfo.FamilyMartStoreCode;
                storeName = checkoutInfo.FamilyMartStoreName;
                storeAddress = checkoutInfo.FamilyMartStoreAddress;
            }

            // ==================== 建立訂單（包含完整資訊）====================
            var order = new Order
            {
                // 基本資訊
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                OrderItems = orderItems,

                // 收件人資訊
                ReceiverName = checkoutInfo.ReceiverName,
                ReceiverPhone = checkoutInfo.ReceiverPhone,

                // 收件地址
                City = checkoutInfo.City,
                District = checkoutInfo.District,
                PostalCode = checkoutInfo.PostalCode,
                Address = checkoutInfo.Address,

                // 配送資訊
                DeliveryMethod = checkoutInfo.DeliveryMethod,
                StoreCode = storeCode,
                StoreName = storeName,
                StoreAddress = storeAddress,

                // 付款資訊
                PaymentMethod = checkoutInfo.PaymentMethod,

                // 發票資訊
                InvoiceType = checkoutInfo.InvoiceType,
                CompanyTaxId = checkoutInfo.CompanyTaxId,
                CompanyName = checkoutInfo.CompanyName,
                InvoiceCarrier = checkoutInfo.InvoiceCarrier,

                // 其他資訊
                OrderNote = checkoutInfo.OrderNote
            };

            // ==================== 建立訂單（使用交易確保原子性）====================
            // 在單一交易中完成：建立訂單、扣除庫存、清空購物車
            // 確保資料一致性，避免並發問題導致超賣或資料不一致
            var createdOrder = await _orderRepository.CreateOrderWithTransactionAsync(order, userId);

            return createdOrder;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            return await _orderRepository.GetOrdersByUserIdAsync(userId);
        }

        public async Task<Order?> GetOrderDetailAsync(int orderId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            var order = await _orderRepository.GetOrderByIdAsync(orderId);

            if (order == null)
                return null;

            // 驗證訂單是否屬於該使用者
            if (order.UserId != userId)
                throw new UnauthorizedAccessException("無權限查看此訂單");

            return order;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new InvalidOperationException($"找不到訂單 ID: {orderId}");

            order.Status = status;
            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task CancelOrderAsync(int orderId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            var order = await _orderRepository.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new InvalidOperationException($"找不到訂單 ID: {orderId}");

            // 驗證訂單是否屬於該使用者
            if (order.UserId != userId)
                throw new UnauthorizedAccessException("無權限取消此訂單");

            // 只有待處理或已付款狀態的訂單可以取消
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Paid)
                throw new InvalidOperationException($"訂單狀態為「{order.Status}」，無法取消");

            // 恢復庫存
            foreach (var orderItem in order.OrderItems)
            {
                var album = await _albumRepository.GetAlbumByIdAsync(orderItem.AlbumId);
                if (album != null)
                {
                    album.Stock += orderItem.Quantity;
                    await _albumRepository.UpdateAlbumAsync(album);
                }
            }

            // 更新訂單狀態
            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<bool> IsOrderOwnedByUserAsync(int orderId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            return await _orderRepository.IsOrderOwnedByUserAsync(orderId, userId);
        }
    }
}
