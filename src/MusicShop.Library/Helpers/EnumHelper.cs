using MusicShop.Data.Entities;
using MusicShop.Data.Entities;

namespace MusicShop.Library.Helpers;

/// <summary>
/// Enum 工具類別，提供統一的 Enum 顯示文字轉換
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// 取得配送方式顯示文字
    /// </summary>
    public static string GetDeliveryMethodText(DeliveryMethod method)
    {
        return method switch
        {
            DeliveryMethod.HomeDelivery => "宅配到府",
            DeliveryMethod.SevenEleven => "7-11 超商取貨",
            DeliveryMethod.FamilyMart => "全家超商取貨",
            _ => "未知"
        };
    }

    /// <summary>
    /// 取得付款方式顯示文字
    /// </summary>
    public static string GetPaymentMethodText(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.CashOnDelivery => "貨到付款",
            PaymentMethod.CreditCard => "信用卡線上刷卡",
            _ => "未知"
        };
    }

    /// <summary>
    /// 取得發票類型顯示文字
    /// </summary>
    public static string GetInvoiceTypeText(InvoiceType type)
    {
        return type switch
        {
            InvoiceType.Duplicate => "二聯式發票（個人）",
            InvoiceType.Triplicate => "三聯式發票（公司）",
            InvoiceType.EInvoice => "電子發票",
            _ => "未知"
        };
    }

    /// <summary>
    /// 取得訂單狀態顯示文字
    /// </summary>
    public static string GetOrderStatusText(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "待處理",
            OrderStatus.Paid => "已付款",
            OrderStatus.Shipped => "已出貨",
            OrderStatus.Completed => "已完成",
            OrderStatus.Cancelled => "已取消",
            _ => "未知"
        };
    }

    /// <summary>
    /// 取得訂單狀態顏色類別（Bootstrap）
    /// </summary>
    public static string GetOrderStatusBadgeClass(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "bg-warning",
            OrderStatus.Paid => "bg-info",
            OrderStatus.Shipped => "bg-primary",
            OrderStatus.Completed => "bg-success",
            OrderStatus.Cancelled => "bg-danger",
            _ => "bg-secondary"
        };
    }
}
