using MusicShop.Data.Entities;
using MusicShop.Library.Enums;

namespace MusicShop.Data.Repositories.Interfaces;

public interface ICouponRepository : IGenericRepository<Coupon>
{
    /// <summary>
    /// 根據兌換碼取得優惠券
    /// </summary>
    Task<Coupon?> GetByCodeAsync(string code);

    /// <summary>
    /// 取得所有優惠券（Admin 列表，依建立時間降序）
    /// </summary>
    Task<IEnumerable<Coupon>> GetAllOrderedAsync();

    /// <summary>
    /// 取得使用者所有優惠券（Include Coupon）
    /// </summary>
    Task<IEnumerable<UserCoupon>> GetUserCouponsAsync(string userId);

    /// <summary>
    /// 根據 ID 取得使用者優惠券（Include Coupon）
    /// </summary>
    Task<UserCoupon?> GetUserCouponByIdAsync(int id);

    /// <summary>
    /// 取得使用者可用的優惠券（未用、未過期，Include Coupon）
    /// </summary>
    Task<IEnumerable<UserCoupon>> GetAvailableUserCouponsAsync(string userId);

    /// <summary>
    /// 檢查使用者是否已領取過特定優惠券（指定來源、指定年份）
    /// </summary>
    Task<bool> HasUserReceivedCouponAsync(string userId, int couponId, CouponSource source, int year);

    /// <summary>
    /// 批次取得已領取過特定優惠券的使用者 ID 集合（指定來源、指定年份）
    /// 用於避免在迴圈中逐一查詢造成 N+1 問題
    /// </summary>
    Task<HashSet<string>> GetReceivedUserIdsAsync(int couponId, CouponSource source, int year);

    /// <summary>
    /// 新增使用者優惠券
    /// </summary>
    Task AddUserCouponAsync(UserCoupon userCoupon);

    /// <summary>
    /// 批次新增使用者優惠券
    /// </summary>
    Task AddUserCouponsRangeAsync(IEnumerable<UserCoupon> userCoupons);

    /// <summary>
    /// 更新使用者優惠券
    /// </summary>
    Task UpdateUserCouponAsync(UserCoupon userCoupon);
}
