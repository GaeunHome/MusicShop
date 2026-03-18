namespace MusicShop.Service.ViewModels.Account;

/// <summary>
/// 帳號首頁最近訂單 ViewModel（展示層使用）
/// 用於 Account/Index.cshtml 頁面的最近訂單列表
/// </summary>
public class RecentOrderViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount => TotalAmount - DiscountAmount;
    public string StatusText { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = string.Empty;
    public string PaymentStatusText { get; set; } = string.Empty;
    public string DeliveryStatusText { get; set; } = string.Empty;
    public bool IsDelivered { get; set; }
    public bool IsCompleted { get; set; }

    // 第一筆商品摘要（帳號首頁使用）
    public string? FirstItemTitle { get; set; }
    public int? FirstItemQuantity { get; set; }
    public int TotalItemCount { get; set; }
    public bool HasMoreItems => TotalItemCount > 1;
    public int ExtraItemCount => TotalItemCount > 1 ? TotalItemCount - 1 : 0;

    public string FormattedTotalAmount => TotalAmount.ToString("N0");
    public string FormattedFinalAmount => FinalAmount.ToString("N0");
    public bool HasDiscount => DiscountAmount > 0;
    public string FormattedOrderDate => OrderDate.ToString("yyyy-MM-dd");
}
