using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 後台發放優惠券 ViewModel
/// </summary>
public class CouponIssueViewModel
{
    public int CouponId { get; set; }
    public string CouponName { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入使用者 Email")]
    [EmailAddress(ErrorMessage = "請輸入有效的 Email")]
    [Display(Name = "使用者 Email")]
    public string UserEmail { get; set; } = string.Empty;
}
