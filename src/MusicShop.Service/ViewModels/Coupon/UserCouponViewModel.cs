using MusicShop.Library.Enums;

namespace MusicShop.Service.ViewModels.Coupon;

/// <summary>
/// 使用者「我的優惠券」頁面 ViewModel
/// </summary>
public class UserCouponViewModel
{
    public int Id { get; set; }
    public string CouponName { get; set; } = string.Empty;
    public string? CouponDescription { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public int? OrderId { get; set; }
    public CouponSource Source { get; set; }

    /// <summary>
    /// 是否已過期
    /// </summary>
    public bool IsExpired => !IsUsed && ExpiresAt <= DateTime.UtcNow;

    /// <summary>
    /// 是否可用
    /// </summary>
    public bool IsAvailable => !IsUsed && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// 折扣描述文字
    /// </summary>
    public string DiscountText => DiscountType == DiscountType.FixedAmount
        ? $"折 NT$ {DiscountValue:N0}"
        : MaxDiscountAmount.HasValue
            ? $"打 {(100 - DiscountValue) / 10m:G} 折（最高折 NT$ {MaxDiscountAmount:N0}）"
            : $"打 {(100 - DiscountValue) / 10m:G} 折";

    /// <summary>
    /// 來源文字
    /// </summary>
    public string SourceText => Source switch
    {
        CouponSource.AdminGrant => "管理員發放",
        CouponSource.CodeRedemption => "兌換碼兌換",
        CouponSource.BirthdayGift => "生日禮物",
        _ => "未知"
    };

    /// <summary>
    /// 狀態文字
    /// </summary>
    public string StatusText => IsUsed ? "已使用" : IsExpired ? "已過期" : "可使用";

    /// <summary>
    /// 狀態 Badge Class
    /// </summary>
    public string StatusBadgeClass => IsUsed ? "bg-secondary" : IsExpired ? "bg-danger" : "bg-success";
}
