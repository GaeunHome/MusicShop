namespace MusicShop.Service.ViewModels.Admin
{
    /// <summary>
    /// 後台幻燈片列表顯示 ViewModel。
    /// 用於後台幻燈片管理列表頁，包含顯示所需的摘要欄位及計算屬性。
    /// 遵循三層式架構：View 層只使用 ViewModel，不直接接觸 Data 層實體。
    /// </summary>
    public class BannerListItemViewModel
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public int? AlbumId { get; set; }
        public string? AlbumTitle { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// 是否有上傳圖片
        /// </summary>
        public bool HasImage => !string.IsNullOrEmpty(ImageUrl);

        /// <summary>
        /// 是否有連結的專輯
        /// </summary>
        public bool HasLinkedAlbum => AlbumId.HasValue && !string.IsNullOrEmpty(AlbumTitle);
    }
}
