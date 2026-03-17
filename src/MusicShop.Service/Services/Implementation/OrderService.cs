using AutoMapper;
using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Library.Helpers;
using MusicShop.Library.Enums;
using MusicShop.Service.ViewModels.Account;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Cart;
using MusicShop.Service.ViewModels.Order;

namespace MusicShop.Service.Services.Implementation
{
    /// <summary>
    /// 訂單商業邏輯實作
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderValidationService _orderValidationService;
        private readonly IMapper _mapper;

        public OrderService(
            IUnitOfWork unitOfWork,
            IOrderValidationService orderValidationService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _orderValidationService = orderValidationService;
            _mapper = mapper;
        }

        public async Task<int> CreateOrderWithFullInfoAsync(string userId, CheckoutViewModel checkoutInfo)
        {
            // ==================== 參數驗證 ====================
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            // 使用驗證服務驗證結帳資訊（避免重複程式碼）
            _orderValidationService.ValidateCheckoutInfo(checkoutInfo);

            // ==================== 取得購物車項目 ====================
            var cartItems = await _unitOfWork.Cart.GetCartItemsByUserIdAsync(userId);

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
            // 交易流程說明：以下三個步驟必須在同一筆交易中完成，順序不可調換。
            // 步驟 1 先建立訂單，確保訂單記錄存在後才執行庫存扣除（步驟 2），
            // 最後清空購物車（步驟 3）。若任一步驟失敗則全部回滾，
            // 避免「庫存已扣但訂單未建立」或「購物車已清但訂單失敗」等資料不一致情況。
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. 建立訂單（必須先建立，後續步驟依賴訂單記錄存在）
                var createdOrder = await _unitOfWork.Orders.CreateOrderAsync(order);

                // 2. 扣除庫存（使用驗證服務統一處理，內含樂觀並發控制）
                await _orderValidationService.DeductStockAsync(orderItems, albumCache);

                // 3. 清空購物車（放在最後，確保訂單與庫存都正確後才清除）
                await _unitOfWork.Cart.ClearCartAsync(userId);

                // 4. 提交交易
                await _unitOfWork.CommitAsync();

                return createdOrder.Id;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            return await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
        }

        public async Task<Order?> GetOrderDetailAsync(int orderId, string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
                return null;

            // 驗證訂單是否屬於該使用者
            if (order.UserId != userId)
                throw new UnauthorizedAccessException("無權限查看此訂單");

            return order;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _unitOfWork.Orders.GetOrderByIdAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _unitOfWork.Orders.GetAllOrdersAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new InvalidOperationException($"找不到訂單 ID: {orderId}");

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Orders.UpdateOrderAsync(order);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelOrderAsync(int orderId, string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new InvalidOperationException($"找不到訂單 ID: {orderId}");

            // 驗證訂單是否屬於該使用者
            if (order.UserId != userId)
                throw new UnauthorizedAccessException("無權限取消此訂單");

            // 只有待處理或已付款狀態的訂單可以取消
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Paid)
                throw new InvalidOperationException($"訂單狀態為「{order.Status}」，無法取消");

            // 取消訂單需在交易中同時完成「恢復庫存」與「更新狀態」，
            // 這是 CreateOrderWithFullInfoAsync 扣庫存的反向操作。
            // 若只更新狀態而未恢復庫存，會導致商品永久少算；
            // 若庫存已恢復但狀態更新失敗，則可能造成重複恢復。
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. 恢復庫存：將每筆訂單明細的數量加回對應專輯的庫存
                //    使用 null 檢查是因為專輯可能已被管理員刪除（此時無需恢復）
                foreach (var orderItem in order.OrderItems)
                {
                    var album = await _unitOfWork.Albums.GetAlbumByIdAsync(orderItem.AlbumId);
                    if (album != null)
                    {
                        album.Stock += orderItem.Quantity;
                        await _unitOfWork.Albums.UpdateAlbumAsync(album);
                    }
                }

                // 2. 更新訂單狀態
                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Orders.UpdateOrderAsync(order);

                // 3. 提交交易
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> IsOrderOwnedByUserAsync(int orderId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            return await _unitOfWork.Orders.IsOrderOwnedByUserAsync(orderId, userId);
        }

        public async Task<List<OrderListItemViewModel>> GetOrderListViewModelsByUserAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);

            return orders.Select(order =>
            {
                var topItems = _mapper.Map<List<OrderItemSummaryViewModel>>(order.OrderItems.Take(3).ToList());

                return new OrderListItemViewModel
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    StatusText = OrderHelper.GetOrderStatusText(order.Status),
                    StatusBadgeClass = OrderHelper.GetOrderStatusBadgeClass(order.Status),
                    PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                    DeliveryMethodText = OrderHelper.GetDeliveryMethodText(order.DeliveryMethod),
                    PaymentStatusText = OrderHelper.GetPaymentStatusText(order.PaymentMethod, order.Status),
                    CanCancel = order.Status == OrderStatus.Pending || order.Status == OrderStatus.Paid,
                    Items = topItems,
                    TotalItemCount = order.OrderItems.Count
                };
            }).ToList();
        }

        public async Task<OrderDetailViewModel?> GetOrderDetailViewModelAsync(int orderId, string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
                return null;

            // 驗證訂單是否屬於該使用者
            if (order.UserId != userId)
                throw new UnauthorizedAccessException("無權限查看此訂單");

            var items = _mapper.Map<List<OrderItemViewModel>>(order.OrderItems.ToList());

            return new OrderDetailViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                StatusText = OrderHelper.GetOrderStatusText(order.Status),
                StatusBadgeClass = OrderHelper.GetOrderStatusBadgeClass(order.Status),
                StatusDescription = OrderHelper.GetOrderStatusDescription(order.Status),
                IsPending = order.Status == OrderStatus.Pending,
                CanCancel = order.Status == OrderStatus.Pending || order.Status == OrderStatus.Paid,
                PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                DeliveryMethodText = OrderHelper.GetDeliveryMethodText(order.DeliveryMethod),
                PaymentStatusText = OrderHelper.GetPaymentStatusText(order.PaymentMethod, order.Status),
                DeliveryStatusText = OrderHelper.GetDeliveryStatusText(order.Status),
                Items = items
            };
        }

        public async Task<OrderConfirmationViewModel?> GetOrderConfirmationViewModelAsync(int orderId, string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
                return null;

            // 驗證訂單是否屬於該使用者
            if (order.UserId != userId)
                throw new UnauthorizedAccessException("無權限查看此訂單");

            var items = _mapper.Map<List<OrderConfirmationItemViewModel>>(order.OrderItems.ToList());

            return new OrderConfirmationViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                DeliveryMethodText = OrderHelper.GetDeliveryMethodText(order.DeliveryMethod),
                StatusText = OrderHelper.GetOrderStatusText(order.Status),
                Items = items
            };
        }

        public async Task<List<AdminOrderListItemViewModel>> GetAdminOrderListViewModelsAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllOrdersAsync();

            return orders.Select(order => new AdminOrderListItemViewModel
            {
                Id = order.Id,
                UserEmail = order.User?.Email ?? "未知",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                PaymentBadgeClass = OrderHelper.GetPaymentStatusText(order.PaymentMethod, order.Status) == "已收到款項" ? "bg-success" : "bg-warning text-dark",
                StatusText = OrderHelper.GetOrderStatusText(order.Status),
                StatusBadgeClass = OrderHelper.GetOrderStatusBadgeClass(order.Status)
            }).ToList();
        }

        public async Task<AdminOrderDetailViewModel?> GetAdminOrderDetailViewModelAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);
            if (order == null) return null;

            var items = _mapper.Map<List<OrderItemViewModel>>(order.OrderItems.ToList());

            var validNextStatuses = OrderHelper.GetValidNextStatuses(order.Status, order.PaymentMethod);

            return new AdminOrderDetailViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                UserEmail = order.User?.Email ?? "未知",
                PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                PaymentBadgeClass = OrderHelper.GetPaymentStatusText(order.PaymentMethod, order.Status) == "已收到款項" ? "bg-success" : "bg-warning text-dark",
                DeliveryMethodText = OrderHelper.GetDeliveryMethodText(order.DeliveryMethod),
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone,
                FullAddress = BuildFullAddress(order),
                Items = items,
                CurrentStatusText = OrderHelper.GetOrderStatusText(order.Status),
                CurrentStatusBadgeClass = OrderHelper.GetOrderStatusBadgeClass(order.Status),
                CurrentStatusDescription = OrderHelper.GetOrderStatusDescription(order.Status),
                CanUpdateStatus = OrderHelper.CanUpdateStatus(order.Status),
                ValidNextStatuses = validNextStatuses.Select(s => new AdminOrderStatusOptionViewModel
                {
                    StatusValue = (int)s.Status,
                    FullText = s.GetFullText(),
                    IsCurrentStatus = s.Status == order.Status
                }).ToList()
            };
        }

        public async Task<(int TotalOrders, decimal TotalSpent)> GetUserOrderStatsAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            return await _unitOfWork.Orders.GetUserOrderStatsAsync(userId);
        }

        /// <summary>
        /// 組合完整地址
        /// 超商取貨與宅配的地址格式不同：
        /// - 超商取貨：顯示「門市名稱 (門市地址)」，因為收件人是到門市自取
        /// - 宅配到府：將郵遞區號、縣市、區域、詳細地址串接為完整地址
        /// </summary>
        private static string BuildFullAddress(Order order)
        {
            // 有門市名稱代表超商取貨，優先使用門市資訊
            if (!string.IsNullOrEmpty(order.StoreName))
            {
                return $"{order.StoreName} ({order.StoreAddress})";
            }

            // 宅配：串接各地址欄位，過濾空值避免多餘空白
            var parts = new[] { order.PostalCode, order.City, order.District, order.Address };
            return string.Join("", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        public async Task<List<RecentOrderViewModel>> GetRecentOrderViewModelsAsync(string userId, int count = 5)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);

            return orders.Take(count).Select(order =>
            {
                var firstItem = order.OrderItems.FirstOrDefault();

                return new RecentOrderViewModel
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    StatusText = OrderHelper.GetOrderStatusText(order.Status),
                    StatusBadgeClass = OrderHelper.GetOrderStatusBadgeClass(order.Status),
                    PaymentStatusText = OrderHelper.GetPaymentStatusText(order.PaymentMethod, order.Status),
                    DeliveryStatusText = OrderHelper.GetDeliveryStatusText(order.Status),
                    IsDelivered = order.Status == OrderStatus.Shipped,
                    IsCompleted = order.Status == OrderStatus.Completed,
                    FirstItemTitle = firstItem?.Album?.Title,
                    FirstItemQuantity = firstItem?.Quantity,
                    TotalItemCount = order.OrderItems.Count
                };
            }).ToList();
        }
    }
}
