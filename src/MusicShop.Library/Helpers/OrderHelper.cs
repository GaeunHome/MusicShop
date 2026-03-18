using MusicShop.Library.Enums;

namespace MusicShop.Library.Helpers
{
    /// <summary>
    /// 訂單相關的輔助方法
    /// 集中管理訂單業務邏輯，避免在 View 層撰寫複雜邏輯
    /// </summary>
    public static class OrderHelper
    {
        /// <summary>
        /// 取得付款狀態的顯示文字
        /// 根據付款方式和訂單狀態共同判斷
        /// </summary>
        public static string GetPaymentStatusText(PaymentMethod paymentMethod, OrderStatus status)
        {
            return paymentMethod switch
            {
                // 信用卡：付款後即算已付款
                PaymentMethod.CreditCard => status switch
                {
                    OrderStatus.Pending => "未付款",
                    OrderStatus.Cancelled => "已取消",
                    _ => "已收到款項"
                },
                // 貨到付款：完成訂單後才算收到款項
                PaymentMethod.CashOnDelivery => status switch
                {
                    OrderStatus.Completed => "已收到款項",
                    OrderStatus.Cancelled => "已取消",
                    _ => "未收款（貨到付款）"
                },
                _ => "未知"
            };
        }

        /// <summary>
        /// 取得配送狀態的顯示文字
        /// </summary>
        public static string GetDeliveryStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "未出貨",
                OrderStatus.Paid => "未出貨",
                OrderStatus.Shipped => "已出貨",
                OrderStatus.Completed => "已完成",
                OrderStatus.Cancelled => "-",
                _ => "未知"
            };
        }

        /// <summary>
        /// 取得訂單狀態的顯示文字
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
                _ => status.ToString()
            };
        }

        /// <summary>
        /// 取得訂單狀態的說明文字
        /// </summary>
        public static string GetOrderStatusDescription(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "訂單已建立，等待處理中",
                OrderStatus.Paid => "已確認收到款項，準備出貨",
                OrderStatus.Shipped => "商品已寄出，等待客戶收貨",
                OrderStatus.Completed => "訂單已完成，交易結束",
                OrderStatus.Cancelled => "訂單已取消",
                _ => ""
            };
        }

        /// <summary>
        /// 取得訂單狀態的 Badge CSS 類別
        /// </summary>
        public static string GetOrderStatusBadgeClass(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "bg-warning text-dark",
                OrderStatus.Paid => "bg-info",
                OrderStatus.Shipped => "bg-primary",
                OrderStatus.Completed => "bg-success",
                OrderStatus.Cancelled => "bg-danger",
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// 取得付款狀態的 Badge CSS 類別
        /// </summary>
        public static string GetPaymentBadgeClass(PaymentMethod paymentMethod, OrderStatus status)
        {
            var statusText = GetPaymentStatusText(paymentMethod, status);
            return statusText == "已收到款項" ? "bg-success" : "bg-warning text-dark";
        }

        /// <summary>
        /// 取得付款方式的顯示文字
        /// </summary>
        public static string GetPaymentMethodText(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.CashOnDelivery => "貨到付款",
                PaymentMethod.CreditCard => "信用卡",
                _ => "未知"
            };
        }

        /// <summary>
        /// 取得配送方式的顯示文字
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
        /// 取得發票類型的顯示文字
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
        /// 取得訂單的有效下一步狀態選項
        /// 根據當前狀態和付款方式，返回合理的下一步選項
        /// </summary>
        public static List<OrderStatusOption> GetValidNextStatuses(OrderStatus status, PaymentMethod paymentMethod)
        {
            var options = new List<OrderStatusOption>();
            var isCashOnDelivery = paymentMethod == PaymentMethod.CashOnDelivery;

            // 狀態轉換規則說明：
            // 正常流程（信用卡）：Pending → Paid → Shipped → Completed
            // 正常流程（貨到付款）：Pending → Shipped → Completed（跳過 Paid，因為付款發生在收貨時）
            // 已完成與已取消為終態，不可再變更。
            switch (status)
            {
                case OrderStatus.Pending:
                    options.Add(new OrderStatusOption(OrderStatus.Pending, "保持現狀"));
                    if (!isCashOnDelivery)
                    {
                        // 信用卡：需先確認收款才能出貨
                        options.Add(new OrderStatusOption(OrderStatus.Paid, "確認收到信用卡款項"));
                    }
                    else
                    {
                        // 貨到付款：不存在「已付款」階段，直接跳到出貨
                        options.Add(new OrderStatusOption(OrderStatus.Shipped, "貨到付款 - 直接出貨"));
                    }
                    options.Add(new OrderStatusOption(OrderStatus.Cancelled, "取消訂單"));
                    break;

                case OrderStatus.Paid:
                    // 已付款狀態只有信用卡訂單才會進入，下一步為出貨
                    options.Add(new OrderStatusOption(OrderStatus.Paid, "保持現狀"));
                    options.Add(new OrderStatusOption(OrderStatus.Shipped, "商品已寄出"));
                    options.Add(new OrderStatusOption(OrderStatus.Cancelled, "取消訂單"));
                    break;

                case OrderStatus.Shipped:
                    options.Add(new OrderStatusOption(OrderStatus.Shipped, "保持現狀"));
                    // 貨到付款的完成代表「收貨 + 付款」同時發生，因此文字不同
                    options.Add(new OrderStatusOption(
                        OrderStatus.Completed,
                        isCashOnDelivery ? "客戶已收貨並付款" : "客戶已收貨"
                    ));
                    break;

                // 終態：已完成與已取消不提供其他選項
                case OrderStatus.Completed:
                    options.Add(new OrderStatusOption(OrderStatus.Completed, "交易完成"));
                    break;

                case OrderStatus.Cancelled:
                    options.Add(new OrderStatusOption(OrderStatus.Cancelled, "已取消"));
                    break;
            }

            return options;
        }

        /// <summary>
        /// 判斷訂單是否可以更新狀態
        /// </summary>
        public static bool CanUpdateStatus(OrderStatus status)
        {
            return status != OrderStatus.Completed && status != OrderStatus.Cancelled;
        }
    }

    /// <summary>
    /// 訂單狀態選項（用於下拉選單）
    /// </summary>
    public class OrderStatusOption
    {
        public OrderStatus Status { get; set; }
        public string Description { get; set; }

        public OrderStatusOption(OrderStatus status, string description)
        {
            Status = status;
            Description = description;
        }

        public string GetLabel()
        {
            return OrderHelper.GetOrderStatusText(Status);
        }

        public string GetFullText()
        {
            return $"{GetLabel()} ({Description})";
        }
    }
}
