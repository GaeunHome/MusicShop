using MusicShop.Library.Enums;
using MusicShop.Service.ViewModels.Order;

namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 後台訂單詳情 ViewModel（Admin/Order/Detail.cshtml 使用）
/// 取代直接在 View 中使用 Data 層 Order Entity
/// </summary>
public class AdminOrderDetailViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 訂單狀態列舉值（供時間軸元件使用）
    /// </summary>
    public OrderStatus Status { get; set; }

    // 會員資訊
    public string UserEmail { get; set; } = string.Empty;

    // 付款與配送（已格式化文字）
    public string PaymentMethodText { get; set; } = string.Empty;
    public string PaymentBadgeClass { get; set; } = string.Empty;
    public string DeliveryMethodText { get; set; } = string.Empty;

    // 收件人資訊
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;

    // 訂單明細
    public List<OrderItemViewModel> Items { get; set; } = new();

    // 狀態相關
    public string CurrentStatusText { get; set; } = string.Empty;
    public string CurrentStatusBadgeClass { get; set; } = string.Empty;
    public string CurrentStatusDescription { get; set; } = string.Empty;
    public bool CanUpdateStatus { get; set; }

    // 有效的下一步狀態選項（供下拉選單使用）
    public List<AdminOrderStatusOptionViewModel> ValidNextStatuses { get; set; } = new();

    public string FormattedTotalAmount => TotalAmount.ToString("N0");
    public string FormattedOrderDate => OrderDate.ToString("yyyy/MM/dd HH:mm");
}

/// <summary>
/// 後台訂單狀態選項 ViewModel（取代 View 直接使用 OrderStatusOption）
/// </summary>
public class AdminOrderStatusOptionViewModel
{
    public int StatusValue { get; set; }
    public string FullText { get; set; } = string.Empty;
    public bool IsCurrentStatus { get; set; }
}
