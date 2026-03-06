using MusicShop.Models;
using MusicShop.Repositories.Interface;
using MusicShop.Services.Interface;
using MusicShop.Helpers;

namespace MusicShop.Services.Implementation
{
    /// <summary>
    /// 購物車商業邏輯實作
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IAlbumRepository _albumRepository;

        public CartService(ICartRepository cartRepository, IAlbumRepository albumRepository)
        {
            _cartRepository = cartRepository;
            _albumRepository = albumRepository;
        }

        public async Task<IEnumerable<CartItem>> GetUserCartAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            return await _cartRepository.GetCartItemsByUserIdAsync(userId);
        }

        public async Task<CartItem> AddToCartAsync(string userId, int albumId, int quantity = 1)
        {
            // 驗證參數
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));
            ValidationHelper.ValidatePositive(quantity, "數量", nameof(quantity));

            // 檢查專輯是否存在
            var album = await _albumRepository.GetAlbumByIdAsync(albumId);
            ValidationHelper.ValidateEntityExists(album, "專輯", albumId);

            // 檢查庫存
            ValidationHelper.ValidateCondition(
                album!.Stock >= quantity,
                $"庫存不足，目前庫存: {album.Stock}"
            );

            // 檢查購物車中是否已有該專輯
            var existingCartItem = await _cartRepository.GetCartItemByUserAndAlbumAsync(userId, albumId);

            if (existingCartItem != null)
            {
                // 已存在，增加數量
                var newQuantity = existingCartItem.Quantity + quantity;

                // 再次檢查庫存
                ValidationHelper.ValidateCondition(
                    album.Stock >= newQuantity,
                    $"庫存不足，目前庫存: {album.Stock}，購物車已有: {existingCartItem.Quantity}"
                );

                existingCartItem.Quantity = newQuantity;
                await _cartRepository.UpdateCartItemAsync(existingCartItem);
                return existingCartItem;
            }
            else
            {
                // 不存在，新增項目
                var cartItem = new CartItem
                {
                    UserId = userId,
                    AlbumId = albumId,
                    Quantity = quantity,
                    AddedAt = DateTime.UtcNow
                };

                return await _cartRepository.AddToCartAsync(cartItem);
            }
        }

        public async Task UpdateCartItemQuantityAsync(int cartItemId, string userId, int quantity)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));
            ValidationHelper.ValidatePositive(quantity, "數量", nameof(quantity));

            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
            ValidationHelper.ValidateEntityExists(cartItem, "購物車項目", cartItemId);

            // 驗證是否為該使用者的購物車項目
            ValidationHelper.ValidateCondition(
                cartItem!.UserId == userId,
                "無權限修改此購物車項目"
            );

            // 檢查庫存
            var album = await _albumRepository.GetAlbumByIdAsync(cartItem.AlbumId);
            ValidationHelper.ValidateEntityExists(album, "專輯", cartItem.AlbumId);

            ValidationHelper.ValidateCondition(
                album!.Stock >= quantity,
                $"庫存不足，目前庫存: {album.Stock}"
            );

            cartItem.Quantity = quantity;
            await _cartRepository.UpdateCartItemAsync(cartItem);
        }

        public async Task RemoveFromCartAsync(int cartItemId, string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
            ValidationHelper.ValidateEntityExists(cartItem, "購物車項目", cartItemId);

            // 驗證是否為該使用者的購物車項目
            ValidationHelper.ValidateCondition(
                cartItem!.UserId == userId,
                "無權限刪除此購物車項目"
            );

            await _cartRepository.RemoveCartItemAsync(cartItemId);
        }

        public async Task ClearCartAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            await _cartRepository.ClearCartAsync(userId);
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId);

            return cartItems.Sum(item => item.Album!.Price * item.Quantity);
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId);

            return cartItems.Sum(item => item.Quantity);
        }
    }
}
