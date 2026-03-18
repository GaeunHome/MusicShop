using MusicShop.Library.Enums;

namespace MusicShop.Service.ViewModels.Coupon;

/// <summary>
/// 結帳頁面可用優惠券下拉選項 ViewModel
/// </summary>
public class AvailableCouponViewModel
{
    public int UserCouponId { get; set; }
    public string CouponName { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 下拉選單顯示文字
    /// </summary>
    public string DisplayText
    {
        get
        {
            var discount = DiscountType == DiscountType.FixedAmount
                ? $"折 NT$ {DiscountValue:N0}"
                : $"打 {(100 - DiscountValue) / 10m:G} 折";
            var expiry = ExpiresAt.ToLocalTime().ToString("MM/dd");
            return $"{CouponName}（{discount}，{expiry} 到期）";
        }
    }
}
