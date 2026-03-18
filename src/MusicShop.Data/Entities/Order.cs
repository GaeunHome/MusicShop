using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MusicShop.Library.Enums;

namespace MusicShop.Data.Entities
{
    /// <summary>
    /// 訂單主檔
    /// </summary>
    public class Order : ISoftDeletable
    {
        public int Id { get; set; }

        // ==================== 基本資訊 ====================
        [Required]
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

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

        // ==================== 收件地址（條件必填：宅配到府時使用）====================
        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? District { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

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

        // ==================== 優惠券資訊 ====================
        /// <summary>
        /// 使用的優惠券 ID（可為 null）
        /// </summary>
        public int? UserCouponId { get; set; }
        public UserCoupon? UserCoupon { get; set; }

        /// <summary>
        /// 折扣金額
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        // ==================== 其他資訊 ====================
        [StringLength(1000)]
        public string? OrderNote { get; set; }

        // ==================== 導航屬性 ====================
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // ==================== 並發控制 ====================
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // ==================== 軟刪除欄位 ====================
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}