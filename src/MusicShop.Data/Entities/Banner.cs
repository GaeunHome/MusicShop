namespace MusicShop.Data.Entities
{
    /// <summary>
    /// 首頁幻燈片橫幅
    /// </summary>
    public class Banner
    {
        public int Id { get; set; }

        /// <summary>
        /// 幻燈片圖片 URL
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// 連結的商品 ID（可為 null，表示純展示不跳轉）
        /// </summary>
        public int? AlbumId { get; set; }
        public Album? Album { get; set; }

        /// <summary>
        /// 顯示順序（數字越小越前面）
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// 是否顯示
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
