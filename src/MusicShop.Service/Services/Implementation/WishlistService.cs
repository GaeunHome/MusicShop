using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Helpers;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Service.Services.Implementation
{
    /// <summary>
    /// 收藏清單業務邏輯實作
    /// </summary>
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WishlistService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<WishlistItem>> GetWishlistAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));
            return await _unitOfWork.Wishlists.GetByUserIdAsync(userId);
        }

        public async Task<bool> ToggleWishlistAsync(string userId, int albumId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));
            ValidationHelper.ValidatePositive(albumId, "專輯 ID", nameof(albumId));

            var existing = await _unitOfWork.Wishlists.GetByUserAndAlbumAsync(userId, albumId);
            if (existing != null)
            {
                await _unitOfWork.Wishlists.RemoveAsync(existing);
                return false; // 已取消收藏
            }

            var album = await _unitOfWork.Albums.GetAlbumByIdAsync(albumId);
            ValidationHelper.ValidateEntityExists(album, "專輯", albumId);

            await _unitOfWork.Wishlists.AddAsync(new WishlistItem
            {
                UserId = userId,
                AlbumId = albumId,
                AddedAt = DateTime.UtcNow
            });

            return true; // 已加入收藏
        }

        public async Task<HashSet<int>> GetWishlistAlbumIdsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return [];

            return await _unitOfWork.Wishlists.GetAlbumIdsByUserAsync(userId);
        }
    }
}
