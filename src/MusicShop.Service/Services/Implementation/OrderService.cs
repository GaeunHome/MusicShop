using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ICouponService _couponService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork,
            IOrderValidationService orderValidationService,
            ICouponService couponService,
            IMapper mapper,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _orderValidationService = orderValidationService;
            _couponService = couponService;
            _mapper = mapper;
            _logger = logger;
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

            // ==================== 優惠券折扣計算 ====================
            decimal discountAmount = 0;
            int? userCouponId = null;

            if (checkoutInfo.UserCouponId.HasValue)
            {
                var couponResult = await _couponService.ValidateAndCalculateDiscountAsync(
                    checkoutInfo.UserCouponId.Value, userId, totalAmount);

                if (couponResult.Success)
                {
                    discountAmount = couponResult.DiscountAmount;
                    userCouponId = checkoutInfo.UserCouponId.Value;
                }
            }

            // ==================== 建立訂單（包含完整資訊）====================
            var order = new Order
            {
                // 基本資訊
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                UserCouponId = userCouponId,
                OrderItems = orderItems,

                // 收件人資訊
                ReceiverName = checkoutInfo.ReceiverName,
                ReceiverPhone = checkoutInfo.ReceiverPhone,
                ContactEmail = checkoutInfo.ContactEmail,

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
            // 交易流程說明：以下步驟必須在同一筆交易中完成，順序不可調換。
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. 建立訂單（必須先建立，後續步驟依賴訂單記錄存在）
                var createdOrder = await _unitOfWork.Orders.CreateOrderAsync(order);

                // 先儲存以取得自動產生的訂單 ID（交易內的 SaveChanges 不會提交，
                // 只是讓 SQL Server 產生 Identity 值，供後續步驟引用）
                await _unitOfWork.SaveChangesAsync();

                // 2. 扣除庫存（使用驗證服務統一處理，內含樂觀並發控制）
                await _orderValidationService.DeductStockAsync(orderItems, albumCache);

                // 3. 標記優惠券已使用
                if (userCouponId.HasValue)
                    await _couponService.MarkCouponAsUsedAsync(userCouponId.Value, createdOrder.Id);

                // 4. 清空購物車（放在最後，確保訂單與庫存都正確後才清除）
                await _unitOfWork.Cart.ClearCartAsync(userId);

                // 5. 提交交易
                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "訂單建立成功：OrderId={OrderId}, UserId={UserId}, TotalAmount={TotalAmount}, DiscountAmount={DiscountAmount}, Items={ItemCount}",
                    createdOrder.Id, userId, totalAmount, discountAmount, orderItems.Count);

                return createdOrder.Id;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogWarning(ex, "訂單建立失敗（庫存並發衝突）：UserId={UserId}", userId);
                throw new InvalidOperationException("庫存已被其他訂單更新，請重新確認購物車後再次下單。", ex);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "訂單建立失敗：UserId={UserId}, TotalAmount={TotalAmount}", userId, totalAmount);
                throw;
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new InvalidOperationException($"找不到訂單 ID: {orderId}");

            // 後台取消訂單時，需要恢復庫存和退還優惠券（與前台 CancelOrderAsync 相同邏輯）
            if (status == OrderStatus.Cancelled &&
                order.Status != OrderStatus.Cancelled)
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    // 1. 恢復庫存與退還優惠券
                    await RestoreOrderResourcesAsync(order);

                    // 2. 更新訂單狀態
                    order.Status = status;
                    order.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Orders.UpdateOrderAsync(order);

                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "後台取消訂單：OrderId={OrderId}, 已恢復庫存並退還優惠券", orderId);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "後台取消訂單失敗：OrderId={OrderId}", orderId);
                    throw;
                }

                return;
            }

            var previousStatus = order.Status;
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Orders.UpdateOrderAsync(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "訂單狀態更新：OrderId={OrderId}, {PreviousStatus} → {NewStatus}",
                orderId, previousStatus, status);
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

                // 1. 恢復庫存與退還優惠券
                await RestoreOrderResourcesAsync(order);

                // 2. 更新訂單狀態
                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Orders.UpdateOrderAsync(order);

                // 4. 提交交易
                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "使用者取消訂單：OrderId={OrderId}, UserId={UserId}, 已恢復庫存{CouponInfo}",
                    orderId, userId, order.UserCouponId.HasValue ? "並退還優惠券" : "");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "使用者取消訂單失敗：OrderId={OrderId}, UserId={UserId}", orderId, userId);
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

            // 自動取消逾時未付款的信用卡訂單
            await AutoCancelExpiredCreditCardOrdersAsync(userId);

            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);

            return orders.Select(order =>
            {
                var topItems = _mapper.Map<List<OrderItemSummaryViewModel>>(order.OrderItems.Take(DisplayConstants.OrderItemsPreviewCount).ToList());

                return new OrderListItemViewModel
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    DiscountAmount = order.DiscountAmount,
                    StatusText = OrderHelper.GetOrderStatusText(order.Status, order.PaymentMethod),
                    StatusBadgeClass = OrderHelper.GetOrderStatusBadgeClass(order.Status),
                    PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                    DeliveryMethodText = OrderHelper.GetDeliveryMethodText(order.DeliveryMethod),
                    PaymentStatusText = OrderHelper.GetPaymentStatusText(order.PaymentMethod, order.Status),
                    CanCancel = order.Status == OrderStatus.Pending || order.Status == OrderStatus.Paid,
                    CanRetryPayment = OrderHelper.CanRetryPayment(order.Status, order.PaymentMethod),
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
                DiscountAmount = order.DiscountAmount,
                Status = order.Status,
                StatusText = OrderHelper.GetOrderStatusText(order.Status, order.PaymentMethod),
                StatusBadgeClass = OrderHelper.GetOrderStatusBadgeClass(order.Status),
                StatusDescription = OrderHelper.GetOrderStatusDescription(order.Status, order.PaymentMethod),
                IsPending = order.Status == OrderStatus.Pending,
                CanCancel = order.Status == OrderStatus.Pending || order.Status == OrderStatus.Paid,
                CanRetryPayment = OrderHelper.CanRetryPayment(order.Status, order.PaymentMethod),
                PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                DeliveryMethodText = OrderHelper.GetDeliveryMethodText(order.DeliveryMethod),
                PaymentStatusText = OrderHelper.GetPaymentStatusText(order.PaymentMethod, order.Status),
                DeliveryStatusText = OrderHelper.GetDeliveryStatusText(order.Status),
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone,
                ContactEmail = order.ContactEmail,
                FullAddress = BuildFullAddress(order),
                OrderNote = order.OrderNote,
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
                DiscountAmount = order.DiscountAmount,
                PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                DeliveryMethodText = OrderHelper.GetDeliveryMethodText(order.DeliveryMethod),
                StatusText = OrderHelper.GetOrderStatusText(order.Status),
                StatusBadgeClass = OrderHelper.GetOrderStatusBadgeClass(order.Status),
                Items = items
            };
        }

        public async Task<List<AdminOrderListItemViewModel>> GetAdminOrderListViewModelsAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllOrdersAsync();

            return orders.Select(order => new AdminOrderListItemViewModel
            {
                Id = order.Id,
                UserEmail = order.User?.Email ?? DisplayConstants.Unknown,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                PaymentBadgeClass = OrderHelper.GetPaymentBadgeClass(order.PaymentMethod, order.Status),
                StatusText = OrderHelper.GetOrderStatusText(order.Status),
                StatusBadgeClass = OrderHelper.GetOrderStatusBadgeClass(order.Status)
            }).ToList();
        }

        public async Task<PagedResult<AdminOrderListItemViewModel>> GetAdminOrderListPagedAsync(int page, int pageSize)
        {
            var (orders, totalCount) = await _unitOfWork.Orders.GetOrdersPagedAsync(page, pageSize);

            var items = orders.Select(order => new AdminOrderListItemViewModel
            {
                Id = order.Id,
                UserEmail = order.User?.Email ?? DisplayConstants.Unknown,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                PaymentBadgeClass = OrderHelper.GetPaymentBadgeClass(order.PaymentMethod, order.Status),
                StatusText = OrderHelper.GetOrderStatusText(order.Status),
                StatusBadgeClass = OrderHelper.GetOrderStatusBadgeClass(order.Status)
            });

            return new PagedResult<AdminOrderListItemViewModel>(items, totalCount, page, pageSize);
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
                UpdatedAt = order.UpdatedAt,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                Status = order.Status,
                UserEmail = order.User?.Email ?? DisplayConstants.Unknown,
                UserFullName = order.User?.FullName,
                PaymentMethodText = OrderHelper.GetPaymentMethodText(order.PaymentMethod),
                PaymentBadgeClass = OrderHelper.GetPaymentBadgeClass(order.PaymentMethod, order.Status),
                PaymentStatusText = OrderHelper.GetPaymentStatusText(order.PaymentMethod, order.Status),
                DeliveryMethodText = OrderHelper.GetDeliveryMethodText(order.DeliveryMethod),
                DeliveryStatusText = OrderHelper.GetDeliveryStatusText(order.Status),
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone,
                ContactEmail = order.ContactEmail,
                FullAddress = BuildFullAddress(order),
                InvoiceTypeText = OrderHelper.GetInvoiceTypeText(order.InvoiceType),
                CompanyTaxId = order.CompanyTaxId,
                CompanyName = order.CompanyName,
                InvoiceCarrier = order.InvoiceCarrier,
                CouponName = order.UserCoupon?.Coupon?.Name,
                OrderNote = order.OrderNote,
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
        /// 自動取消逾時未付款的信用卡訂單
        /// 信用卡訂單超過 30 分鐘仍為 Pending 狀態，自動取消並恢復庫存
        /// </summary>
        private async Task AutoCancelExpiredCreditCardOrdersAsync(string userId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
            var cutoffTime = DateTime.UtcNow.AddMinutes(-OrderHelper.CreditCardPaymentTimeoutMinutes);

            var expiredOrders = orders.Where(o =>
                o.Status == OrderStatus.Pending &&
                o.PaymentMethod == PaymentMethod.CreditCard &&
                o.OrderDate < cutoffTime).ToList();

            foreach (var order in expiredOrders)
            {
                try
                {
                    // 需要追蹤的實體，重新從 DB 取得（GetOrdersByUserIdAsync 用了 AsNoTracking）
                    var trackedOrder = await _unitOfWork.Orders.GetOrderByIdAsync(order.Id);
                    if (trackedOrder == null || trackedOrder.Status != OrderStatus.Pending) continue;

                    await _unitOfWork.BeginTransactionAsync();

                    await RestoreOrderResourcesAsync(trackedOrder);

                    trackedOrder.Status = OrderStatus.Cancelled;
                    trackedOrder.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Orders.UpdateOrderAsync(trackedOrder);

                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "信用卡訂單逾時自動取消：OrderId={OrderId}, 建立時間={OrderDate}",
                        trackedOrder.Id, trackedOrder.OrderDate);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "自動取消逾時訂單失敗：OrderId={OrderId}", order.Id);
                }
            }
        }

        /// <summary>
        /// 恢復訂單的庫存與優惠券（取消訂單時的反向操作）
        /// 將每筆訂單明細的數量加回對應專輯的庫存，並退還已使用的優惠券。
        /// 注意：此方法不包含交易管理，呼叫端需自行處理交易。
        /// </summary>
        private async Task RestoreOrderResourcesAsync(Order order)
        {
            // 恢復庫存：使用 null 檢查是因為專輯可能已被管理員刪除（此時無需恢復）
            foreach (var orderItem in order.OrderItems)
            {
                var album = await _unitOfWork.Albums.GetAlbumByIdAsync(orderItem.AlbumId);
                if (album != null)
                {
                    album.Stock += orderItem.Quantity;
                    await _unitOfWork.Albums.UpdateAlbumAsync(album);
                }
            }

            // 退還優惠券（若有使用）
            if (order.UserCouponId.HasValue)
                await _couponService.ReleaseCouponAsync(order.UserCouponId.Value);
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

        public async Task<List<RecentOrderViewModel>> GetRecentOrderViewModelsAsync(string userId, int count = DisplayConstants.RecentOrdersDefaultCount)
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
                    DiscountAmount = order.DiscountAmount,
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
