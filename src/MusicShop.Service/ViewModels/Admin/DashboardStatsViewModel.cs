namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 後台儀表板統計資料 ViewModel
/// 將多個獨立查詢合併為單次服務呼叫，減少 Controller 與 Service 間的來回次數
/// </summary>
public class DashboardStatsViewModel
{
    public int AlbumCount { get; set; }
    public int ArtistCount { get; set; }
    public int CategoryCount { get; set; }
    public int OrderCount { get; set; }
    public int UserCount { get; set; }
    public decimal TotalSales { get; set; }
    public int PendingOrderCount { get; set; }
    public int BannerCount { get; set; }
    public int FeaturedArtistCount { get; set; }
    public int CouponCount { get; set; }
}
