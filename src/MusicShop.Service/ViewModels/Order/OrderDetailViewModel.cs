using MusicShop.Library.Enums;

namespace MusicShop.Service.ViewModels.Order;

/// <summary>
/// 訂單詳情 ViewModel（展示層使用）
/// 用於 Order/Detail.cshtml 頁面
/// </summary>
public class OrderDetailViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount => TotalAmount - DiscountAmount;

    // 狀態相關
    /// <summary>
    /// 訂單狀態列舉值（供時間軸元件使用）
    /// </summary>
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public bool IsPending { get; set; }
    public bool CanCancel { get; set; }
    public bool CanRetryPayment { get; set; }

    // 付款與配送
    public string PaymentMethodText { get; set; } = string.Empty;
    public string DeliveryMethodText { get; set; } = string.Empty;
    public string PaymentStatusText { get; set; } = string.Empty;
    public string DeliveryStatusText { get; set; } = string.Empty;

    // 收件人資訊
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;

    // 配送地址
    public string FullAddress { get; set; } = string.Empty;

    // 訂單備註
    public string? OrderNote { get; set; }

    // 訂單明細
    public List<OrderItemViewModel> Items { get; set; } = new();
    public int TotalQuantity => Items.Sum(i => i.Quantity);

    public string FormattedTotalAmount => TotalAmount.ToString("N0");
    public string FormattedDiscountAmount => DiscountAmount.ToString("N0");
    public string FormattedFinalAmount => FinalAmount.ToString("N0");
    public bool HasDiscount => DiscountAmount > 0;
    public string FormattedOrderDate => OrderDate.ToString("yyyy-MM-dd HH:mm:ss");
}

/// <summary>
/// 訂單明細項目 ViewModel
/// </summary>
public class OrderItemViewModel
{
    public int AlbumId { get; set; }
    public string AlbumTitle { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal => UnitPrice * Quantity;
    public string FormattedUnitPrice => UnitPrice.ToString("N0");
    public string FormattedSubTotal => SubTotal.ToString("N0");
}
