using System.ComponentModel.DataAnnotations.Schema;
using MusicShop.Library.Enums;

namespace MusicShop.Data.Entities;

/// <summary>
/// 使用者持有的優惠券（交易性質，硬刪除）
/// </summary>
public class UserCoupon
{
    public int Id { get; set; }

    /// <summary>
    /// 持有者
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }

    /// <summary>
    /// 優惠券模板
    /// </summary>
    public int CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    /// <summary>
    /// 發放時間
    /// </summary>
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 到期時間
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 是否已使用
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// 使用時間
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// 使用的訂單 ID
    /// </summary>
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    /// <summary>
    /// 優惠券來源
    /// </summary>
    public CouponSource Source { get; set; }
}
