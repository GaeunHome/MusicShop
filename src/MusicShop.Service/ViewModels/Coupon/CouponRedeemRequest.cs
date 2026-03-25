namespace MusicShop.Service.ViewModels.Coupon;

/// <summary>
/// 優惠券兌換的 API 請求模型
/// </summary>
public class CouponRedeemRequest
{
    public string Code { get; set; } = string.Empty;
}
