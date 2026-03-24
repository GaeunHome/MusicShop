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

    /// <summary>
    /// 將百分比折扣值轉換為台灣「X 折」格式
    /// 例如：DiscountValue = 10（折 10%）→ 回傳 "9 折"
    ///       DiscountValue = 15（折 15%）→ 回傳 "8.5 折"
    /// </summary>
    /// <param name="discountPercentage">折扣百分比（例如 10 代表折 10%）</param>
    /// <returns>台灣折數文字（例如 "9 折"）</returns>
    public static string ToTaiwanDiscount(this decimal discountPercentage)
    {
        var fold = (100 - discountPercentage) / 10m;
        return $"{fold:G} 折";
    }
}
