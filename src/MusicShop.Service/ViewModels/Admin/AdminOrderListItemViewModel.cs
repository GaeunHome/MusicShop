namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 後台訂單列表項目 ViewModel（Admin/Order/Index.cshtml 使用）
/// 取代直接在 View 中使用 Data 層 Order Entity
/// </summary>
public class AdminOrderListItemViewModel
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount => TotalAmount - DiscountAmount;

    // 已格式化的顯示文字（由 Service 層透過 OrderHelper 處理）
    public string PaymentMethodText { get; set; } = string.Empty;
    public string PaymentBadgeClass { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = string.Empty;

    public string FormattedTotalAmount => TotalAmount.ToString("N0");
    public string FormattedFinalAmount => FinalAmount.ToString("N0");
    public bool HasDiscount => DiscountAmount > 0;
    public string FormattedOrderDate => OrderDate.ToString("yyyy/MM/dd HH:mm");
}
