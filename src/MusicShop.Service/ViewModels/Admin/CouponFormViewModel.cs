using System.ComponentModel.DataAnnotations;
using MusicShop.Library.Enums;

namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 後台優惠券新增/編輯表單 ViewModel
/// </summary>
public class CouponFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "請輸入兌換碼")]
    [StringLength(50, ErrorMessage = "兌換碼不可超過 50 字元")]
    [Display(Name = "兌換碼")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入優惠券名稱")]
    [StringLength(100, ErrorMessage = "名稱不可超過 100 字元")]
    [Display(Name = "優惠券名稱")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "描述")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "請選擇折扣類型")]
    [Display(Name = "折扣類型")]
    public DiscountType DiscountType { get; set; }

    [Required(ErrorMessage = "請輸入折扣值")]
    [Range(0.01, 99999, ErrorMessage = "折扣值必須大於 0")]
    [Display(Name = "折扣值")]
    public decimal DiscountValue { get; set; }

    [Display(Name = "最高折扣金額（百分比折扣用）")]
    public decimal? MaxDiscountAmount { get; set; }

    [Range(1, 365, ErrorMessage = "有效天數須在 1~365 之間")]
    [Display(Name = "有效天數")]
    public int ValidDays { get; set; } = 30;

    [Display(Name = "是否啟用")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "可兌換碼兌換")]
    public bool IsRedeemable { get; set; } = false;
}
