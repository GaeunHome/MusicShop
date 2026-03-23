using MusicShop.Library.Helpers;

namespace MusicShop.Service.ViewModels.Order;

/// <summary>
/// 訂單列表項目 ViewModel（展示層使用）
/// 用於 Order/Index.cshtml 列表頁面，包含顯示所需的所有欄位
/// </summary>
public class OrderListItemViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount => TotalAmount - DiscountAmount;

    // 已格式化的顯示文字（由 Service 層處理）
    public string StatusText { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = string.Empty;
    public string PaymentMethodText { get; set; } = string.Empty;
    public string DeliveryMethodText { get; set; } = string.Empty;
    public string PaymentStatusText { get; set; } = string.Empty;

    // 是否可取消（待處理或已付款狀態）
    public bool CanCancel { get; set; }
    // 是否可重新付款（信用卡 + 待處理）
    public bool CanRetryPayment { get; set; }

    // 訂單商品摘要（最多顯示 3 筆）
    public List<OrderItemSummaryViewModel> Items { get; set; } = new();
    public int TotalItemCount { get; set; }
    public bool HasMoreItems => TotalItemCount > DisplayConstants.OrderItemsPreviewCount;
    public int ExtraItemCount => TotalItemCount > DisplayConstants.OrderItemsPreviewCount
        ? TotalItemCount - DisplayConstants.OrderItemsPreviewCount : 0;

    public string FormattedTotalAmount => TotalAmount.ToString("N0");
    public string FormattedDiscountAmount => DiscountAmount.ToString("N0");
    public string FormattedFinalAmount => FinalAmount.ToString("N0");
    public bool HasDiscount => DiscountAmount > 0;
    public string FormattedOrderDate => OrderDate.ToString("yyyy-MM-dd HH:mm");
}

/// <summary>
/// 訂單商品摘要（列表頁使用）
/// </summary>
public class OrderItemSummaryViewModel
{
    public string AlbumTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string FormattedUnitPrice => UnitPrice.ToString("N0");
}
