namespace MusicShop.Service.ViewModels.Coupon;

/// <summary>
/// 優惠券驗證的 API 請求模型
/// </summary>
public class CouponValidateRequest
{
    public int UserCouponId { get; set; }
    public decimal TotalAmount { get; set; }
}
