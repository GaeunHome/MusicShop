using MusicShop.Models;
using MusicShop.Repositories.Interface;
using MusicShop.Services.Interface;

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
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            if (quantity <= 0)
                throw new ArgumentException("數量必須大於 0", nameof(quantity));

            // 檢查專輯是否存在
            var album = await _albumRepository.GetAlbumByIdAsync(albumId);
            if (album == null)
                throw new InvalidOperationException($"找不到專輯 ID: {albumId}");

            // 檢查庫存
            if (album.Stock < quantity)
                throw new InvalidOperationException($"庫存不足，目前庫存: {album.Stock}");

            // 檢查購物車中是否已有該專輯
            var existingCartItem = await _cartRepository.GetCartItemByUserAndAlbumAsync(userId, albumId);

            if (existingCartItem != null)
            {
                // 已存在，增加數量
                var newQuantity = existingCartItem.Quantity + quantity;

                // 再次檢查庫存
                if (album.Stock < newQuantity)
                    throw new InvalidOperationException($"庫存不足，目前庫存: {album.Stock}，購物車已有: {existingCartItem.Quantity}");

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
                    AddedAt = DateTime.Now
                };

                return await _cartRepository.AddToCartAsync(cartItem);
            }
        }

        public async Task UpdateCartItemQuantityAsync(int cartItemId, string userId, int quantity)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            if (quantity <= 0)
                throw new ArgumentException("數量必須大於 0", nameof(quantity));

            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);

            if (cartItem == null)
                throw new InvalidOperationException($"找不到購物車項目 ID: {cartItemId}");

            // 驗證是否為該使用者的購物車項目
            if (cartItem.UserId != userId)
                throw new UnauthorizedAccessException("無權限修改此購物車項目");

            // 檢查庫存
            var album = await _albumRepository.GetAlbumByIdAsync(cartItem.AlbumId);
            if (album == null)
                throw new InvalidOperationException("找不到對應的專輯");

            if (album.Stock < quantity)
                throw new InvalidOperationException($"庫存不足，目前庫存: {album.Stock}");

            cartItem.Quantity = quantity;
            await _cartRepository.UpdateCartItemAsync(cartItem);
        }

        public async Task RemoveFromCartAsync(int cartItemId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);

            if (cartItem == null)
                throw new InvalidOperationException($"找不到購物車項目 ID: {cartItemId}");

            // 驗證是否為該使用者的購物車項目
            if (cartItem.UserId != userId)
                throw new UnauthorizedAccessException("無權限刪除此購物車項目");

            await _cartRepository.RemoveCartItemAsync(cartItemId);
        }

        public async Task ClearCartAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            await _cartRepository.ClearCartAsync(userId);
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

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
