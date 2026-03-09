namespace MusicShop.Helpers;

/// <summary>
/// 價格格式化工具類別
/// 提供統一的價格格式化方法
/// </summary>
public static class PriceFormatter
{
    /// <summary>
    /// 格式化價格為千分位，無小數點（例：1,200）
    /// </summary>
    /// <param name="price">價格</param>
    /// <returns>格式化後的價格字串</returns>
    public static string Format(decimal price)
    {
        return price.ToString("N0");
    }

    /// <summary>
    /// 格式化價格為千分位，含貨幣符號（例：NT$ 1,200）
    /// </summary>
    /// <param name="price">價格</param>
    /// <param name="currencySymbol">貨幣符號，預設為 NT$</param>
    /// <returns>格式化後的價格字串</returns>
    public static string FormatWithCurrency(decimal price, string currencySymbol = "NT$")
    {
        return $"{currencySymbol} {price.ToString("N0")}";
    }

    /// <summary>
    /// 批次格式化價格清單
    /// </summary>
    /// <param name="prices">價格清單</param>
    /// <returns>格式化後的價格字串清單</returns>
    public static List<string> FormatBatch(IEnumerable<decimal> prices)
    {
        return prices.Select(Format).ToList();
    }
}
