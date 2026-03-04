using MusicShop.Models;
using MusicShop.Repositories.Interface;
using MusicShop.Services.Interface;

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

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IAlbumRepository albumRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _albumRepository = albumRepository;
        }

        public async Task<Order> CreateOrderFromCartAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            // 取得購物車項目
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId);
            var cartItemsList = cartItems.ToList();

            if (!cartItemsList.Any())
                throw new InvalidOperationException("購物車是空的，無法建立訂單");

            // 驗證庫存並計算總金額
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();
            var albumCache = new Dictionary<int, Album>(); // 快取已查詢的專輯，避免重複查詢

            foreach (var cartItem in cartItemsList)
            {
                var album = await _albumRepository.GetAlbumByIdAsync(cartItem.AlbumId);

                if (album == null)
                    throw new InvalidOperationException($"找不到專輯 ID: {cartItem.AlbumId}");

                // 檢查庫存
                if (album.Stock < cartItem.Quantity)
                    throw new InvalidOperationException($"專輯「{album.Title}」庫存不足，目前庫存: {album.Stock}");

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

            // 建立訂單
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow, // 使用 UTC 時間避免時區問題
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                OrderItems = orderItems
            };

            // 儲存訂單
            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            // 扣除庫存（使用快取的專輯物件，避免重複查詢資料庫）
            foreach (var orderItem in orderItems)
            {
                if (albumCache.TryGetValue(orderItem.AlbumId, out var album))
                {
                    album.Stock -= orderItem.Quantity;
                    await _albumRepository.UpdateAlbumAsync(album);
                }
            }

            // 清空購物車
            await _cartRepository.ClearCartAsync(userId);

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
