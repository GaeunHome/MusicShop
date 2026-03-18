using MusicShop.Service.ViewModels.Album;

namespace MusicShop.Service.ViewModels.Home;

/// <summary>
/// 首頁精選藝人展示 ViewModel
/// </summary>
public class FeaturedArtistDisplayViewModel
{
    public int ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public List<AlbumCardViewModel> Albums { get; set; } = new();

    public bool HasProfileImage => !string.IsNullOrEmpty(ProfileImageUrl);
}
