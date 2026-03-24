namespace MusicShop.Service.ViewModels.Account;

/// <summary>
/// 會員中心首頁資料模型
/// </summary>
public class AccountIndexViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime RegisteredAt { get; set; }

    // 兩步驟驗證
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorMethod { get; set; }

    // 統計資訊
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }

    // 近期訂單（最多顯示 5 筆）
    public List<RecentOrderViewModel> RecentOrders { get; set; } = new();

    // 個人資料完整度
    public bool HasEmail => !string.IsNullOrEmpty(Email);
    public bool HasPhone { get; set; }
    public bool HasBirthday { get; set; }

    /// <summary>
    /// 缺少的個人資料項目（用於提醒使用者補充）
    /// </summary>
    public List<string> MissingProfileItems
    {
        get
        {
            var items = new List<string>();
            if (!HasEmail) items.Add("電子郵件");
            if (!HasPhone) items.Add("手機號碼");
            if (!HasBirthday) items.Add("生日");
            return items;
        }
    }

    public bool IsProfileComplete => MissingProfileItems.Count == 0;
}
