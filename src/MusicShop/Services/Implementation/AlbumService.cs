using MusicShop.Models;
using MusicShop.Services.Interface;
using MusicShop.Repositories.Interface;
using MusicShop.Helpers;

namespace MusicShop.Services.Implementation
{
    /// <summary>
    /// 專輯商業邏輯實作
    /// </summary>
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;

        public AlbumService(IAlbumRepository albumRepository)
        {
            _albumRepository = albumRepository;
        }

        public async Task<IEnumerable<Album>> GetAlbumsAsync(
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? artistId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null,
            string? sortBy = null)
        {
            return await _albumRepository.GetAlbumsAsync(searchTerm, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy);
        }

        public async Task<Album?> GetAlbumDetailAsync(int id)
        {
            return await _albumRepository.GetAlbumByIdAsync(id);
        }

        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int count)
        {
            ValidationHelper.ValidatePositive(count, "數量", nameof(count));

            return await _albumRepository.GetLatestAlbumsAsync(count);
        }

        public async Task<bool> IsStockAvailableAsync(int albumId, int quantity = 1)
        {
            ValidationHelper.ValidatePositive(quantity, "數量", nameof(quantity));

            var album = await _albumRepository.GetAlbumByIdAsync(albumId);

            if (album == null)
            {
                return false;
            }

            return album.Stock.HasValue && album.Stock.Value >= quantity;
        }

        public async Task<Album> CreateAlbumAsync(Album album)
        {
            // 商業邏輯驗證
            ValidationHelper.ValidateNotEmpty(album.Title, "專輯標題", nameof(album.Title));
            ValidationHelper.ValidatePositive(album.Price, "價格", nameof(album.Price));

            album.CreatedAt = DateTime.UtcNow;
            return await _albumRepository.AddAlbumAsync(album);
        }

        public async Task UpdateAlbumAsync(Album album)
        {
            // 商業邏輯驗證
            ValidationHelper.ValidateNotEmpty(album.Title, "專輯標題", nameof(album.Title));
            ValidationHelper.ValidatePositive(album.Price, "價格", nameof(album.Price));

            var exists = await _albumRepository.AlbumExistsAsync(album.Id);
            ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {album.Id} 的專輯");

            await _albumRepository.UpdateAlbumAsync(album);
        }

        public async Task DeleteAlbumAsync(int id)
        {
            var exists = await _albumRepository.AlbumExistsAsync(id);
            ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {id} 的專輯");

            await _albumRepository.DeleteAlbumAsync(id);
        }
    }
}
