using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 系統參數表單 ViewModel（新增/編輯共用）
/// </summary>
public class SystemSettingFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "請輸入參數 Key")]
    [StringLength(100, ErrorMessage = "Key 長度不可超過 100 字元")]
    [RegularExpression(@"^[a-z0-9_.]+$", ErrorMessage = "Key 只能包含小寫字母、數字、點號和底線")]
    [Display(Name = "參數 Key")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入參數值")]
    [Display(Name = "參數值")]
    public string Value { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "說明長度不可超過 200 字元")]
    [Display(Name = "說明")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "分組名稱不可超過 50 字元")]
    [Display(Name = "分組")]
    public string? Group { get; set; }

    [Required(ErrorMessage = "請選擇值類型")]
    [Display(Name = "值類型")]
    public string ValueType { get; set; } = "string";
}
