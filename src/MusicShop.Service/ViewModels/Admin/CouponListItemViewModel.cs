using MusicShop.Library.Enums;

namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 後台優惠券列表 ViewModel
/// </summary>
public class CouponListItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int ValidDays { get; set; }
    public bool IsActive { get; set; }
    public bool IsRedeemable { get; set; }
    public int IssuedCount { get; set; }
    public int UsedCount { get; set; }

    /// <summary>
    /// 折扣描述
    /// </summary>
    public string DiscountText => DiscountType == DiscountType.FixedAmount
        ? $"折 NT$ {DiscountValue:N0}"
        : MaxDiscountAmount.HasValue
            ? $"打 {100 - DiscountValue} 折（上限 NT$ {MaxDiscountAmount:N0}）"
            : $"打 {100 - DiscountValue} 折";
}
