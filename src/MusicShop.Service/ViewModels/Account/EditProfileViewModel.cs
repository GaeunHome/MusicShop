using System.ComponentModel.DataAnnotations;
using MusicShop.Library.Enums;

namespace MusicShop.Service.ViewModels.Account;

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

    [Required(ErrorMessage = "請輸入手機號碼")]
    [Phone(ErrorMessage = "手機號碼格式不正確")]
    [RegularExpression(@"^09\d{8}$", ErrorMessage = "請輸入正確的台灣手機號碼格式（09開頭共10碼）")]
    [Display(Name = "手機號碼")]
    public string PhoneNumber { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "生日")]
    public DateTime? Birthday { get; set; }

    [Display(Name = "性別")]
    public Gender? Gender { get; set; }

    [Display(Name = "註冊日期")]
    public DateTime RegisteredAt { get; set; }

    // View 層 helper 屬性
    public bool IsMale => Gender == Library.Enums.Gender.Male;
    public bool IsFemale => Gender == Library.Enums.Gender.Female;
}
