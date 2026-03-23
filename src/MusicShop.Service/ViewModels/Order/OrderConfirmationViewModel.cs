namespace MusicShop.Service.ViewModels.Order;

/// <summary>
/// 訂單完成確認 ViewModel（展示層使用）
/// 用於 Cart/OrderComplete.cshtml 頁面
/// </summary>
public class OrderConfirmationViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount => TotalAmount - DiscountAmount;
    public string PaymentMethodText { get; set; } = string.Empty;
    public string DeliveryMethodText { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = string.Empty;

    // 訂單明細
    public List<OrderConfirmationItemViewModel> Items { get; set; } = new();

    public string FormattedTotalAmount => TotalAmount.ToString("N0");
    public string FormattedDiscountAmount => DiscountAmount.ToString("N0");
    public string FormattedFinalAmount => FinalAmount.ToString("N0");
    public bool HasDiscount => DiscountAmount > 0;
    public string FormattedOrderDate => OrderDate.ToString("yyyy/MM/dd HH:mm");
}

/// <summary>
/// 訂單完成確認頁明細項目 ViewModel
/// </summary>
public class OrderConfirmationItemViewModel
{
    public string AlbumTitle { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal => UnitPrice * Quantity;
    public string FormattedUnitPrice => UnitPrice.ToString("N0");
    public string FormattedSubTotal => SubTotal.ToString("N0");
}
