namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 後台精選藝人列表 ViewModel
/// </summary>
public class FeaturedArtistListItemViewModel
{
    public int Id { get; set; }
    public int ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int AlbumCount { get; set; }

    public bool HasProfileImage => !string.IsNullOrEmpty(ProfileImageUrl);
}
