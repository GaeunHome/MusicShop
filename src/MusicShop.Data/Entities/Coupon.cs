using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MusicShop.Library.Enums;

namespace MusicShop.Data.Entities;

/// <summary>
/// 優惠券模板（定義折扣規則，可多次發放給使用者）
/// </summary>
public class Coupon : ISoftDeletable
{
    public int Id { get; set; }

    /// <summary>
    /// 兌換碼（唯一）
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 優惠券名稱（如：新會員折扣、生日禮）
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 優惠券描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 折扣類型（固定金額 / 百分比）
    /// </summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>
    /// 折扣值（固定金額：元，百分比：1~99）
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// 百分比折扣的最高折扣金額上限（null 表示不限）
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// 有效天數（發放後幾天內到期，預設 30 天）
    /// </summary>
    public int ValidDays { get; set; } = 30;

    /// <summary>
    /// 是否啟用（停用後無法新發放）
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 是否可透過兌換碼兌換（true = 使用者可自行輸入兌換碼領取）
    /// </summary>
    public bool IsRedeemable { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ===== 導航屬性 =====
    public ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();

    // ===== 軟刪除欄位 =====
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
