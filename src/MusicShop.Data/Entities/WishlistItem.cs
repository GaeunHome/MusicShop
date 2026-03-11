namespace MusicShop.Data.Entities
{
    /// <summary>
    /// 收藏清單項目實體
    /// </summary>
    public class WishlistItem
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }

        public int AlbumId { get; set; }
        public Album? Album { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
