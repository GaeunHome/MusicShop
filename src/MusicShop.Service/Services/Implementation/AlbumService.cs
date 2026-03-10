using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Helpers;

namespace MusicShop.Service.Services.Implementation
{
    /// <summary>
    /// 專輯商業邏輯實作
    /// </summary>
    public class AlbumService : IAlbumService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlbumService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Album>> GetAlbumsAsync(
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? artistId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null,
            string? sortBy = null)
        {
            return await _unitOfWork.Albums.GetAlbumsAsync(searchTerm, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy);
        }

        public async Task<Album?> GetAlbumDetailAsync(int id)
        {
            return await _unitOfWork.Albums.GetAlbumByIdAsync(id);
        }

        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int count)
        {
            ValidationHelper.ValidatePositive(count, "數量", nameof(count));

            return await _unitOfWork.Albums.GetLatestAlbumsAsync(count);
        }

        public async Task<bool> IsStockAvailableAsync(int albumId, int quantity = 1)
        {
            ValidationHelper.ValidatePositive(quantity, "數量", nameof(quantity));

            var album = await _unitOfWork.Albums.GetAlbumByIdAsync(albumId);

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
            return await _unitOfWork.Albums.AddAlbumAsync(album);
        }

        public async Task UpdateAlbumAsync(Album album)
        {
            // 商業邏輯驗證
            ValidationHelper.ValidateNotEmpty(album.Title, "專輯標題", nameof(album.Title));
            ValidationHelper.ValidatePositive(album.Price, "價格", nameof(album.Price));

            var exists = await _unitOfWork.Albums.AlbumExistsAsync(album.Id);
            ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {album.Id} 的專輯");

            await _unitOfWork.Albums.UpdateAlbumAsync(album);
        }

        public async Task DeleteAlbumAsync(int id)
        {
            var exists = await _unitOfWork.Albums.AlbumExistsAsync(id);
            ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {id} 的專輯");

            await _unitOfWork.Albums.DeleteAlbumAsync(id);
        }
    }
}
