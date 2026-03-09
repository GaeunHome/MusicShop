using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicShop.Models
{
    /// <summary>
    /// 訂單狀態
    /// </summary>
    public enum OrderStatus
    {
        Pending,    // 待處理
        Paid,       // 已付款
        Shipped,    // 已出貨
        Completed,  // 已完成
        Cancelled   // 已取消
    }

    /// <summary>
    /// 配送方式
    /// </summary>
    public enum DeliveryMethod
    {
        HomeDelivery,   // 宅配到府
        SevenEleven,    // 7-11 超商取貨
        FamilyMart      // 全家超商取貨
    }

    /// <summary>
    /// 付款方式
    /// </summary>
    public enum PaymentMethod
    {
        CashOnDelivery, // 貨到付款
        CreditCard      // 信用卡
    }

    /// <summary>
    /// 發票類型
    /// </summary>
    public enum InvoiceType
    {
        Duplicate,      // 二聯式發票（個人）
        Triplicate,     // 三聯式發票（公司）
        EInvoice        // 電子發票
    }

    /// <summary>
    /// 訂單主檔
    /// </summary>
    public class Order
    {
        public int Id { get; set; }

        // ==================== 基本資訊 ====================
        [Required]
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        // ==================== 收件人資訊 ====================
        [Required]
        [StringLength(100)]
        public string ReceiverName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ReceiverPhone { get; set; } = string.Empty;

        // ==================== 收件地址 ====================
        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string District { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        // ==================== 配送資訊 ====================
        [Required]
        public DeliveryMethod DeliveryMethod { get; set; }

        // 超商門市資訊（超商取貨時使用）
        [StringLength(50)]
        public string? StoreCode { get; set; }

        [StringLength(200)]
        public string? StoreName { get; set; }

        [StringLength(500)]
        public string? StoreAddress { get; set; }

        // ==================== 付款資訊 ====================
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        // ==================== 發票資訊 ====================
        [Required]
        public InvoiceType InvoiceType { get; set; }

        // 三聯式發票欄位
        [StringLength(8)]
        public string? CompanyTaxId { get; set; }

        [StringLength(200)]
        public string? CompanyName { get; set; }

        // 電子發票載具
        [StringLength(100)]
        public string? InvoiceCarrier { get; set; }

        // ==================== 其他資訊 ====================
        [StringLength(1000)]
        public string? OrderNote { get; set; }

        // ==================== 導航屬性 ====================
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}