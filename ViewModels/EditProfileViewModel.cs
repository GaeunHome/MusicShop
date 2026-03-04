using System.ComponentModel.DataAnnotations;

namespace MusicShop.ViewModels;

/// <summary>
/// 編輯個人資料資料模型
/// </summary>
public class EditProfileViewModel
{
    [Required(ErrorMessage = "請輸入姓名")]
    [StringLength(100, ErrorMessage = "姓名長度不可超過 100 個字元")]
    [Display(Name = "姓名")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入電子郵件")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    [Display(Name = "電子郵件")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "註冊日期")]
    public DateTime RegisteredAt { get; set; }
}
