namespace MusicShop.Library.Enums
{
    /// <summary>
    /// 發票類型
    /// </summary>
    public enum InvoiceType
    {
        Duplicate = 0,      // 二聯式發票（個人）
        Triplicate = 1,     // 三聯式發票（公司）
        EInvoice = 2        // 電子發票
    }
}
