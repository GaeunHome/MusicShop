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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 雙分類系統
        /// <summary>
        /// 藝人分類 ID（男子團體、女子團體等）
        /// </summary>
        public int? ArtistCategoryId { get; set; }
        public ArtistCategory? ArtistCategory { get; set; }

        /// <summary>
        /// 商品類型 ID（專輯、手燈、周邊等）
        /// </summary>
        public int? ProductTypeId { get; set; }
        public ProductType? ProductType { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}