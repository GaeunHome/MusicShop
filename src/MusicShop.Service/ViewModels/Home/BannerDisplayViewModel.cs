namespace MusicShop.Service.ViewModels.Home;

/// <summary>
/// 首頁幻燈片顯示 ViewModel（展示層使用）
/// 用於 Home/Index.cshtml 首頁輪播
/// </summary>
public class BannerDisplayViewModel
{
    public int Id { get; set; }
    public string? ImageUrl { get; set; }
    public int? AlbumId { get; set; }
    public bool HasLinkedAlbum => AlbumId.HasValue;
    public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
}
