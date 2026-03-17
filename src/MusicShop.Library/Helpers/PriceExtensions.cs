namespace MusicShop.Library.Helpers;

/// <summary>
/// 價格格式化擴充方法
/// 注意："N0"、"N2" 等數值格式字串依賴 CultureInfo，千分位符號與小數點符號
/// 會隨執行環境的文化設定而改變（例如德語環境下千分位為句點）。
/// 本專案預設部署於繁體中文環境（zh-TW），千分位為逗號，符合台幣顯示需求。
/// 若需跨地區部署，應明確指定 CultureInfo.InvariantCulture 或 "zh-TW"。
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
