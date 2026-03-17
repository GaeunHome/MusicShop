namespace MusicShop.Service.ViewModels.Admin
{
    /// <summary>
    /// 後台藝人列表顯示 ViewModel。
    /// 用於後台藝人管理列表頁，包含顯示所需的摘要欄位。
    /// 遵循三層式架構：View 層只使用 ViewModel，不直接接觸 Data 層實體。
    /// </summary>
    public class ArtistListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ArtistCategoryName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int AlbumCount { get; set; }
    }
}
