using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicShop.Data.Entities
{
    public class Album
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;


        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 商品介紹圖片 URL（用於顯示詳細的商品介紹圖，如組成內容、尺寸等）
        /// </summary>
        public string? DescriptionImageUrl { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public string? CoverImageUrl { get; set; }
        [Required]
        public int Stock { get; set; }

        /// <summary>
        /// 並發控制欄位（用於防止庫存更新時的並發問題）
        /// </summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 雙分類系統
        /// <summary>
        /// 商品類型 ID（專輯、手燈、周邊等）
        /// </summary>
        public int? ProductTypeId { get; set; }
        public ProductType? ProductType { get; set; }

        /// <summary>
        /// 藝人/團體 ID（關聯到具體的藝人團體，如：2PM、BTS 等）
        /// </summary>
        public int? ArtistId { get; set; }

        /// <summary>
        /// 導航屬性：隸屬的藝人/團體
        /// </summary>
        public Artist? Artist { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}