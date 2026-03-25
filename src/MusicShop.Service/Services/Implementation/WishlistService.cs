using AutoMapper;
using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Helpers;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Wishlist;

namespace MusicShop.Service.Services.Implementation
{
    /// <summary>
    /// 收藏清單業務邏輯實作
    /// </summary>
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WishlistService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WishlistItemViewModel>> GetWishlistItemViewModelsAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));
            var items = await _unitOfWork.Wishlists.GetByUserIdAsync(userId);

            return _mapper.Map<IEnumerable<WishlistItemViewModel>>(items);
        }

        public async Task<bool> ToggleWishlistAsync(string userId, int albumId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));
            ValidationHelper.ValidatePositive(albumId, "專輯 ID", nameof(albumId));

            var existingWishItem = await _unitOfWork.Wishlists.GetByUserAndAlbumAsync(userId, albumId);
            if (existingWishItem != null)
            {
                // 已收藏 → 取消收藏
                await _unitOfWork.Wishlists.RemoveAsync(existingWishItem);
                await _unitOfWork.SaveChangesAsync();
                return false;
            }

            // 未收藏 → 加入收藏（先確認專輯存在）
            var targetAlbum = await _unitOfWork.Albums.GetAlbumByIdAsync(albumId);
            ValidationHelper.ValidateEntityExists(targetAlbum, "專輯", albumId);

            await _unitOfWork.Wishlists.AddAsync(new WishlistItem
            {
                UserId = userId,
                AlbumId = albumId,
                AddedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();

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
