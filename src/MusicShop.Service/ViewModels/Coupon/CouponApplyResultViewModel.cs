namespace MusicShop.Service.ViewModels.Coupon;

/// <summary>
/// AJAX 套用優惠券結果 ViewModel
/// </summary>
public class CouponApplyResultViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}
