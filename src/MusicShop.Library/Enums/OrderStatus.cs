namespace MusicShop.Library.Enums;

/// <summary>
/// 訂單狀態
/// </summary>
public enum OrderStatus
{
    Pending = 0,    // 待處理
    Paid = 1,       // 已付款
    Shipped = 2,    // 已出貨
    Completed = 3,  // 已完成
    Cancelled = 4   // 已取消
}
