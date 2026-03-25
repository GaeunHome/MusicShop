namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 使用者管理 ViewModel
/// 用於後台管理頁面顯示使用者資訊和角色
/// </summary>
public class UserManagementViewModel
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 手機號碼
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// 註冊時間
    /// </summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>
    /// Email 是否已驗證
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// 是否已啟用兩步驟驗證
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// 是否為管理員
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// 是否為超級管理員
    /// </summary>
    public bool IsSuperAdmin { get; set; }

    /// <summary>
    /// 使用者擁有的所有角色
    /// </summary>
    public List<string> Roles { get; set; } = new List<string>();
}
