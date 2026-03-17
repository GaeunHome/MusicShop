namespace MusicShop.Web.Infrastructure;

/// <summary>
/// 網站全域設定，從 appsettings.json 的 SiteSettings 區段綁定
/// </summary>
public class SiteSettings
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

    /// <summary>社群媒體連結設定</summary>
    public SocialMediaSettings SocialMedia { get; set; } = new();

    /// <summary>頁尾連結設定</summary>
    public FooterLinkSettings FooterLinks { get; set; } = new();
}

/// <summary>
/// 社群媒體連結設定
/// </summary>
public class SocialMediaSettings
{
    /// <summary>Facebook 粉絲專頁連結</summary>
    public string Facebook { get; set; } = string.Empty;

    /// <summary>Instagram 帳號連結</summary>
    public string Instagram { get; set; } = string.Empty;

    /// <summary>LINE 官方帳號連結</summary>
    public string LINE { get; set; } = string.Empty;
}

/// <summary>
/// 頁尾政策連結設定
/// </summary>
public class FooterLinkSettings
{
    /// <summary>關於我們頁面連結</summary>
    public string About { get; set; } = "#";

    /// <summary>退款政策頁面連結</summary>
    public string RefundPolicy { get; set; } = "#";

    /// <summary>服務條款頁面連結</summary>
    public string Terms { get; set; } = "#";
}
