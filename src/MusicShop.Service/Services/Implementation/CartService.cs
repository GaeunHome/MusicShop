using AutoMapper;
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
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartItem>> GetUserCartAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            return await _unitOfWork.Cart.GetCartItemsByUserIdAsync(userId);
        }

        public async Task<List<CartItemViewModel>> GetCartItemViewModelsAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var cartItems = await _unitOfWork.Cart.GetCartItemsByUserIdAsync(userId);

            return _mapper.Map<List<CartItemViewModel>>(cartItems);
        }
        // async 標記在方法上，表示「這個方法裡面有 await」
        public async Task<CartItem> AddToCartAsync(string userId, int albumId, int quantity = 1)
        {
            // 驗證參數
            // 一開始就驗證參數，不合法就直接拋出例外，避免進入後續邏輯造成不必要的資料庫查詢和運算
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));
            ValidationHelper.ValidatePositive(quantity, "數量", nameof(quantity));

            // 檢查目標專輯是否存在
            // 如果直接注入，一個 Service 可能需要注入 5 個 Repository，構造函數超長
            // UnitOfWork 提供統一入口
            // 所有 Repository 共用同一個 DbContext，SaveChangesAsync() 才能一次提交所有變更
            var targetAlbum = await _unitOfWork.Albums.GetAlbumByIdAsync(albumId);
            ValidationHelper.ValidateEntityExists(targetAlbum, "專輯", albumId);

            // 第一次庫存檢查：快速攔截明顯超量的請求（例如庫存 0 時直接拒絕），
            // 避免不必要的資料庫查詢（查購物車現有項目）。
            ValidationHelper.ValidateCondition(
                targetAlbum!.Stock >= quantity,
                $"庫存不足，目前庫存: {targetAlbum.Stock}"
            );

            // 檢查購物車中是否已有該專輯
            var existingCartItem = await _unitOfWork.Cart.GetCartItemByUserAndAlbumAsync(userId, albumId);

            if (existingCartItem != null)
            {
                // 已存在，增加數量
                var newQuantity = existingCartItem.Quantity + quantity;

                // 第二次庫存檢查：購物車已有此商品時，需驗證「累計數量」是否超過庫存。
                // 例如庫存 3、購物車已有 2、本次加 2 → 總計 4 超過庫存，需在此攔截。
                // 這是第一次檢查無法涵蓋的情境，因為第一次只檢查本次新增的數量。
                ValidationHelper.ValidateCondition(
                    targetAlbum.Stock >= newQuantity,
                    $"庫存不足，目前庫存: {targetAlbum.Stock}，購物車已有: {existingCartItem.Quantity}"
                );

                existingCartItem.Quantity = newQuantity;
                await _unitOfWork.Cart.UpdateCartItemAsync(existingCartItem);
                // 統一由 Service 層呼叫 SaveChangesAsync() 決定什麼時候存
                await _unitOfWork.SaveChangesAsync();
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

                var added = await _unitOfWork.Cart.AddToCartAsync(cartItem);
                // 統一由 Service 層呼叫 SaveChangesAsync() 決定什麼時候存
                await _unitOfWork.SaveChangesAsync();
                return added;
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

            // 取得要更新的購物車項目
            var targetCartItem = await _unitOfWork.Cart.GetCartItemByIdAsync(cartItemId);
            ValidationHelper.ValidateEntityExists(targetCartItem, "購物車項目", cartItemId);

            // 驗證是否為該使用者的購物車項目（防止跨使用者操作）
            ValidationHelper.ValidateCondition(
                targetCartItem!.UserId == userId,
                "無權限修改此購物車項目"
            );

            // 檢查該專輯的庫存是否足夠
            var relatedAlbum = await _unitOfWork.Albums.GetAlbumByIdAsync(targetCartItem.AlbumId);
            ValidationHelper.ValidateEntityExists(relatedAlbum, "專輯", targetCartItem.AlbumId);

            ValidationHelper.ValidateCondition(
                relatedAlbum!.Stock >= quantity,
                $"庫存不足，目前庫存: {relatedAlbum.Stock}"
            );

            // 更新數量並儲存
            targetCartItem.Quantity = quantity;
            await _unitOfWork.Cart.UpdateCartItemAsync(targetCartItem);
            await _unitOfWork.SaveChangesAsync();

            return (targetCartItem, relatedAlbum);
        }

        public async Task<CartUpdateResult> UpdateCartItemQuantityAjaxAsync(int cartItemId, string userId, int quantity)
        {
            try
            {
                // 使用私有方法統一驗證和更新邏輯（避免重複程式碼）
                var (updatedCartItem, updatedAlbum) = await ValidateAndUpdateCartItemAsync(cartItemId, userId, quantity);

                // 計算該商品的小計
                decimal itemSubtotal = updatedAlbum.Price * quantity;

                // ===== 效能優化：一次查詢取得所有購物車資料 =====
                // 避免分別呼叫 GetCartTotalAsync 和 GetCartItemCountAsync
                // 減少從 3 次資料庫查詢降低至 2 次
                var allCartItems = await _unitOfWork.Cart.GetCartItemsByUserIdAsync(userId);

                // 計算購物車總金額（從已查詢的資料在記憶體中計算）
                decimal totalCartAmount = allCartItems.Sum(item => (item.Album?.Price ?? 0) * item.Quantity);

                // 計算購物車商品總數量
                int totalCartQuantity = allCartItems.Sum(item => item.Quantity);

                return new CartUpdateResult
                {
                    Success = true,
                    Message = "數量已更新",
                    Quantity = quantity,
                    Subtotal = itemSubtotal,
                    CartTotal = totalCartAmount,
                    CartItemCount = totalCartQuantity
                };
            }
            catch (ArgumentException ex)
            {
                // 驗證錯誤（參數無效）- 回傳使用者友善訊息
                return new CartUpdateResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch (InvalidOperationException ex)
            {
                // 業務邏輯錯誤（庫存不足等）- 回傳使用者友善訊息
                return new CartUpdateResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch (Exception)
            {
                // 非預期錯誤 - 不洩漏內部訊息
                return new CartUpdateResult
                {
                    Success = false,
                    Message = "更新數量時發生錯誤，請稍後再試"
                };
            }
        }

        public async Task RemoveFromCartAsync(int cartItemId, string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            var targetCartItem = await _unitOfWork.Cart.GetCartItemByIdAsync(cartItemId);
            ValidationHelper.ValidateEntityExists(targetCartItem, "購物車項目", cartItemId);

            // 驗證是否為該使用者的購物車項目（防止跨使用者操作）
            ValidationHelper.ValidateCondition(
                targetCartItem!.UserId == userId,
                "無權限刪除此購物車項目"
            );

            await _unitOfWork.Cart.RemoveCartItemAsync(cartItemId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            await _unitOfWork.Cart.ClearCartAsync(userId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            return await _unitOfWork.Cart.GetCartTotalAsync(userId);
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

            return await _unitOfWork.Cart.GetCartItemCountAsync(userId);
        }
    }
}
