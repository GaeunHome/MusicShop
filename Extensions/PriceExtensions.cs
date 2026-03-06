namespace MusicShop.Extensions;

/// <summary>
/// 價格格式化擴充方法
/// </summary>
public static class PriceExtensions
{
    /// <summary>
    /// 格式化價格為台幣格式（加千分位）
    /// </summary>
    /// <param name="price">價格</param>
    /// <returns>格式化後的價格字串（例如：1,250）</returns>
    public static string ToTaiwanPrice(this decimal price)
    {
        return price.ToString("N0");
    }

    /// <summary>
    /// 格式化價格為台幣格式（加千分位，保留小數）
    /// </summary>
    /// <param name="price">價格</param>
    /// <returns>格式化後的價格字串（例如：1,250.00）</returns>
    public static string ToTaiwanPriceWithDecimal(this decimal price)
    {
        return price.ToString("N2");
    }

    /// <summary>
    /// 格式化價格為完整台幣顯示
    /// </summary>
    /// <param name="price">價格</param>
    /// <returns>格式化後的價格字串（例如：NT$ 1,250）</returns>
    public static string ToFullTaiwanPrice(this decimal price)
    {
        return $"NT$ {price.ToString("N0")}";
    }
}
