namespace MusicShop.Service.ViewModels.Shared;

/// <summary>
/// 網站設定 ViewModel（從資料庫 SystemSettings 組合而成）
/// </summary>
public class SiteSettingsViewModel
{
    /// <summary>網站名稱</summary>
    public string SiteName { get; set; } = "MusicShop";

    /// <summary>客服專線電話</summary>
    public string CustomerServicePhone { get; set; } = string.Empty;

    /// <summary>客服服務時間</summary>
    public string CustomerServiceHours { get; set; } = string.Empty;

    /// <summary>客服信箱</summary>
    public string CustomerServiceEmail { get; set; } = string.Empty;

    /// <summary>公司統一編號</summary>
    public string CompanyTaxId { get; set; } = string.Empty;

    /// <summary>會員編號（顯示於頂部資訊列）</summary>
    public string MembershipId { get; set; } = string.Empty;

    /// <summary>Facebook 粉絲專頁連結</summary>
    public string FacebookUrl { get; set; } = string.Empty;

    /// <summary>Instagram 帳號連結</summary>
    public string InstagramUrl { get; set; } = string.Empty;

    /// <summary>LINE 官方帳號連結</summary>
    public string LineUrl { get; set; } = string.Empty;

    /// <summary>是否進入維護模式</summary>
    public bool MaintenanceMode { get; set; }

    /// <summary>維護模式顯示訊息</summary>
    public string MaintenanceMessage { get; set; } = string.Empty;

    /// <summary>全站公告（空值不顯示）</summary>
    public string Announcement { get; set; } = string.Empty;
}
