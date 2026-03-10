using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels.Cart;

namespace MusicShop.Service.Services.Implementation
{
    /// <summary>
    /// 購物車商業邏輯實作
    /// </summary>
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CartItem>> GetUserCartAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            return await _unitOfWork.Cart.GetCartItemsByUserIdAsync(userId);
        }

        public async Task<CartItem> AddToCartAsync(string userId, int albumId, int quantity = 1)
        {
            // 驗證參數
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));
            ValidationHelper.ValidatePositive(quantity, "數量", nameof(quantity));

            // 檢查專輯是否存在
            var album = await _unitOfWork.Albums.GetAlbumByIdAsync(albumId);
            ValidationHelper.ValidateEntityExists(album, "專輯", albumId);

            // 檢查庫存
            ValidationHelper.ValidateCondition(
                album!.Stock >= quantity,
                $"庫存不足，目前庫存: {album.Stock}"
            );

            // 檢查購物車中是否已有該專輯
            var existingCartItem = await _unitOfWork.Cart.GetCartItemByUserAndAlbumAsync(userId, albumId);

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
                await _unitOfWork.Cart.UpdateCartItemAsync(existingCartItem);
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

                return await _unitOfWork.Cart.AddToCartAsync(cartItem);
            }
        }

        public async Task UpdateCartItemQuantityAsync(int cartItemId, string userId, int quantity)
        {
            // 使用私有方法統一驗證和更新邏輯，避免重複
            await ValidateAndUpdateCartItemAsync(cartItemId, userId, quantity);
        }

        /// <summary>
        /// 私有方法：驗證並更新購物車項目
        /// 此方法統一了 UpdateCartItemQuantityAsync 和 UpdateCartItemQuantityAjaxAsync 的共用邏輯
        /// </summary>
        private async Task<(CartItem CartItem, Album Album)> ValidateAndUpdateCartItemAsync(
            int cartItemId, string userId, int quantity)
        {
            // 參數驗證
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));
            ValidationHelper.ValidatePositive(quantity, "數量", nameof(quantity));

            // 取得購物車項目
            var cartItem = await _unitOfWork.Cart.GetCartItemByIdAsync(cartItemId);
            ValidationHelper.ValidateEntityExists(cartItem, "購物車項目", cartItemId);

            // 驗證是否為該使用者的購物車項目
            ValidationHelper.ValidateCondition(
                cartItem!.UserId == userId,
                "無權限修改此購物車項目"
            );

            // 檢查庫存
            var album = await _unitOfWork.Albums.GetAlbumByIdAsync(cartItem.AlbumId);
            ValidationHelper.ValidateEntityExists(album, "專輯", cartItem.AlbumId);

            ValidationHelper.ValidateCondition(
                album!.Stock >= quantity,
                $"庫存不足，目前庫存: {album.Stock}"
            );

            // 更新數量
            cartItem.Quantity = quantity;
            await _unitOfWork.Cart.UpdateCartItemAsync(cartItem);

            return (cartItem, album);
        }

        public async Task<CartUpdateResult> UpdateCartItemQuantityAjaxAsync(int cartItemId, string userId, int quantity)
        {
            try
            {
                // 使用私有方法統一驗證和更新邏輯（避免重複程式碼）
                var (cartItem, album) = await ValidateAndUpdateCartItemAsync(cartItemId, userId, quantity);

                // 計算小計（該商品）
                decimal subtotal = album.Price * quantity;

                // ===== 效能優化：一次查詢取得所有購物車資料 =====
                // 避免分別呼叫 GetCartTotalAsync 和 GetCartItemCountAsync
                // 減少從 3 次資料庫查詢降低至 2 次
                var allCartItems = await _unitOfWork.Cart.GetCartItemsByUserIdAsync(userId);

                // 計算購物車總金額（從已查詢的資料計算）
                decimal cartTotal = allCartItems.Sum(item => item.Album!.Price * item.Quantity);

                // 計算購物車商品總數量（從已查詢的資料計算）
                int cartItemCount = allCartItems.Sum(item => item.Quantity);

                // 返回成功結果（使用 PriceFormatter 統一格式化）
                return new CartUpdateResult
                {
                    Success = true,
                    Message = "數量已更新",
                    Quantity = quantity,
                    Subtotal = PriceFormatter.Format(subtotal),
                    CartTotal = PriceFormatter.Format(cartTotal),
                    CartItemCount = cartItemCount
                };
            }
            catch (Exception ex)
            {
                // 返回失敗結果
                return new CartUpdateResult
                {
                    Success = false,
                    Message = ex.Message,
                    Quantity = 0,
                    Subtotal = "0",
                    CartTotal = "0",
                    CartItemCount = 0
                };
            }
        }

        public async Task RemoveFromCartAsync(int cartItemId, string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var cartItem = await _unitOfWork.Cart.GetCartItemByIdAsync(cartItemId);
            ValidationHelper.ValidateEntityExists(cartItem, "購物車項目", cartItemId);

            // 驗證是否為該使用者的購物車項目
            ValidationHelper.ValidateCondition(
                cartItem!.UserId == userId,
                "無權限刪除此購物車項目"
            );

            await _unitOfWork.Cart.RemoveCartItemAsync(cartItemId);
        }

        public async Task ClearCartAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            await _unitOfWork.Cart.ClearCartAsync(userId);
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var cartItems = await _unitOfWork.Cart.GetCartItemsByUserIdAsync(userId);

            return cartItems.Sum(item => item.Album!.Price * item.Quantity);
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("使用者 ID 不能為空", nameof(userId));

            var cartItems = await _unitOfWork.Cart.GetCartItemsByUserIdAsync(userId);

            return cartItems.Sum(item => item.Quantity);
        }
    }
}
