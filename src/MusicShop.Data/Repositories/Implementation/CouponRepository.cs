using Microsoft.EntityFrameworkCore;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;
using MusicShop.Library.Enums;

namespace MusicShop.Data.Repositories.Implementation;

/// <summary>
/// 優惠券資料存取實作
/// </summary>
public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
{
    public CouponRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task<IEnumerable<Coupon>> GetAllOrderedAsync()
    {
        return await _context.Coupons
            .Include(c => c.UserCoupons)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserCoupon>> GetUserCouponsAsync(string userId)
    {
        return await _context.UserCoupons
            .Where(uc => uc.UserId == userId)
            .Include(uc => uc.Coupon)
            .OrderByDescending(uc => uc.IssuedAt)
            .ToListAsync();
    }

    public async Task<UserCoupon?> GetUserCouponByIdAsync(int id)
    {
        return await _context.UserCoupons
            .Include(uc => uc.Coupon)
            .FirstOrDefaultAsync(uc => uc.Id == id);
    }

    public async Task<IEnumerable<UserCoupon>> GetAvailableUserCouponsAsync(string userId)
    {
        return await _context.UserCoupons
            .Where(uc => uc.UserId == userId && !uc.IsUsed && uc.ExpiresAt > DateTime.UtcNow)
            .Include(uc => uc.Coupon)
            .OrderBy(uc => uc.ExpiresAt)
            .ToListAsync();
    }

    public async Task<bool> HasUserReceivedCouponAsync(string userId, int couponId, CouponSource source, int year)
    {
        return await _context.UserCoupons
            .AnyAsync(uc => uc.UserId == userId
                && uc.CouponId == couponId
                && uc.Source == source
                && uc.IssuedAt.Year == year);
    }

    public async Task AddUserCouponAsync(UserCoupon userCoupon)
    {
        await _context.UserCoupons.AddAsync(userCoupon);
    }

    public async Task AddUserCouponsRangeAsync(IEnumerable<UserCoupon> userCoupons)
    {
        await _context.UserCoupons.AddRangeAsync(userCoupons);
    }

    public Task UpdateUserCouponAsync(UserCoupon userCoupon)
    {
        _context.UserCoupons.Update(userCoupon);
        return Task.CompletedTask;
    }
}
