namespace MusicShop.Library.Helpers;

/// <summary>
/// 庫存相關擴充方法
/// </summary>
public static class StockExtensions
{
    /// <summary>
    /// 低庫存閾值（庫存數量低於或等於此值時視為庫存不足）
    /// </summary>
    private const int LowStockThreshold = 5;

    /// <summary>
    /// 判斷是否有庫存
    /// </summary>
    public static bool IsInStock(this int stock)
    {
        return stock > 0;
    }

    /// <summary>
    /// 判斷是否庫存不足（低於或等於閾值）
    /// </summary>
    public static bool IsLowStock(this int stock)
    {
        return stock > 0 && stock <= LowStockThreshold;
    }

    /// <summary>
    /// 判斷是否已售完
    /// </summary>
    public static bool IsSoldOut(this int stock)
    {
        return stock <= 0;
    }

    /// <summary>
    /// 取得庫存狀態文字
    /// </summary>
    public static string GetStockStatusText(this int stock)
    {
        if (stock.IsSoldOut())
            return "已售完";
        else if (stock.IsLowStock())
            return $"僅剩 {stock} 件";
        else
            return $"庫存充足（剩餘 {stock} 件）";
    }

    /// <summary>
    /// 取得庫存狀態 CSS 類別
    /// </summary>
    public static string GetStockStatusCssClass(this int stock)
    {
        if (stock.IsSoldOut())
            return "text-danger";
        else if (stock.IsLowStock())
            return "text-warning";
        else
            return "text-success";
    }

    /// <summary>
    /// 取得庫存狀態圖示
    /// </summary>
    public static string GetStockStatusIcon(this int stock)
    {
        if (stock.IsSoldOut())
            return "bi-x-circle-fill";
        else if (stock.IsLowStock())
            return "bi-exclamation-triangle-fill";
        else
            return "bi-check-circle-fill";
    }
}
