using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicShop.Models
{
    public enum OrderStatus
    {
        Pending,    // 待處理
        Paid,       // 已付款
        Shipped,    // 已出貨
        Completed,  // 已完成
        Cancelled   // 已取消
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}