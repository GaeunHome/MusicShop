namespace MusicShop.Library.Helpers;

/// <summary>
/// 日期時間格式化擴充方法
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// 格式化為韓國出貨日期格式
    /// </summary>
    /// <param name="date">日期</param>
    /// <returns>格式化後的日期字串（例如：2026.03.15）</returns>
    public static string ToKoreanShippingDate(this DateTime date)
    {
        return date.ToString("yyyy.MM.dd");
    }

    /// <summary>
    /// 格式化為台灣日期格式
    /// </summary>
    /// <param name="date">日期</param>
    /// <returns>格式化後的日期字串（例如：2026-03-15）</returns>
    public static string ToTaiwanDate(this DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 格式化為台灣日期時間格式
    /// </summary>
    /// <param name="date">日期時間</param>
    /// <returns>格式化後的日期時間字串（例如：2026-03-15 14:30）</returns>
    public static string ToTaiwanDateTime(this DateTime date)
    {
        return date.ToString("yyyy-MM-dd HH:mm");
    }
}
