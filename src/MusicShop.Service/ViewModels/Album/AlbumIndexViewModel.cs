using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicShop.Service.ViewModels.Album
{
    public class AlbumIndexViewModel
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

        // 關聯 ID
        public int? ArtistCategoryId { get; set; }
        public string? ArtistCategoryName { get; set; }

        public int? ProductTypeId { get; set; }
        public string? ProductTypeName { get; set; }

        public string? Search { get; set; }
    }
}
