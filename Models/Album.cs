using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicShop.Models
{
    public class Album
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Artist { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public string? CoverImageUrl { get; set; }

        public int? Stock { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 關聯
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}