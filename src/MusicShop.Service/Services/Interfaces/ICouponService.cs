using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Coupon;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 優惠券商業邏輯介面
/// </summary>
public interface ICouponService
{
    // ==================== Admin CRUD ====================

    /// <summary>
    /// 取得後台優惠券列表
    /// </summary>
    Task<IEnumerable<CouponListItemViewModel>> GetCouponListItemsAsync();

    /// <summary>
    /// 根據 ID 取得優惠券表單
    /// </summary>
    Task<CouponFormViewModel?> GetCouponFormByIdAsync(int id);

    /// <summary>
    /// 新增優惠券
    /// </summary>
    Task CreateCouponAsync(CouponFormViewModel vm);

    /// <summary>
    /// 更新優惠券
    /// </summary>
    Task UpdateCouponAsync(CouponFormViewModel vm);

    /// <summary>
    /// 刪除優惠券
    /// </summary>
    Task DeleteCouponAsync(int id);

    // ==================== 使用者 ====================

    /// <summary>
    /// 取得使用者所有優惠券
    /// </summary>
    Task<IEnumerable<UserCouponViewModel>> GetUserCouponsAsync(string userId);

    /// <summary>
    /// 取得結帳時可用的優惠券
    /// </summary>
    Task<IEnumerable<AvailableCouponViewModel>> GetAvailableCouponsForCheckoutAsync(string userId);

    /// <summary>
    /// 驗證優惠券並計算折扣金額
    /// </summary>
    Task<CouponApplyResultViewModel> ValidateAndCalculateDiscountAsync(int userCouponId, string userId, decimal totalAmount);

    // ==================== 兌換 ====================

    /// <summary>
    /// 使用者透過兌換碼兌換優惠券
    /// </summary>
    Task<(bool Success, string Message)> RedeemCouponByCodeAsync(string userId, string code);

    // ==================== 發放 ====================

    /// <summary>
    /// 管理員發放優惠券給指定使用者
    /// </summary>
    Task<(bool Success, string Message)> IssueCouponToUserAsync(int couponId, string userEmail);

    /// <summary>
    /// 統一發放優惠券給所有使用者
    /// </summary>
    Task<int> IssueCouponToAllUsersAsync(int couponId);

    /// <summary>
    /// 發放生日優惠券給當月壽星
    /// </summary>
    Task<int> IssueBirthdayCouponsAsync(int couponId);

    // ==================== 訂單整合 ====================

    /// <summary>
    /// 標記優惠券已使用（建立訂單時呼叫）
    /// </summary>
    Task MarkCouponAsUsedAsync(int userCouponId, int orderId);

    /// <summary>
    /// 退還優惠券（取消訂單時呼叫）
    /// </summary>
    Task ReleaseCouponAsync(int userCouponId);
}
