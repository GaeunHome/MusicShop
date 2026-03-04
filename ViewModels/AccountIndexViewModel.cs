using MusicShop.Models;

namespace MusicShop.ViewModels;

/// <summary>
/// 會員中心首頁資料模型
/// </summary>
public class AccountIndexViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }

    // 統計資訊
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }

    // 近期訂單（最多顯示 5 筆）
    public List<Order> RecentOrders { get; set; } = new();
}
