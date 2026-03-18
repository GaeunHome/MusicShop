using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Enums;
using MusicShop.Library.Helpers;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Coupon;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 優惠券商業邏輯實作
/// </summary>
public class CouponService : ICouponService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<CouponService> _logger;

    public CouponService(
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        IMapper mapper,
        ILogger<CouponService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    // ==================== Admin CRUD ====================

    public async Task<IEnumerable<CouponListItemViewModel>> GetCouponListItemsAsync()
    {
        var coupons = await _unitOfWork.Coupons.GetAllOrderedAsync();
        return _mapper.Map<IEnumerable<CouponListItemViewModel>>(coupons);
    }

    public async Task<CouponFormViewModel?> GetCouponFormByIdAsync(int id)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
        if (coupon == null) return null;
        return _mapper.Map<CouponFormViewModel>(coupon);
    }

    public async Task CreateCouponAsync(CouponFormViewModel vm)
    {
        // 檢查兌換碼唯一性
        var existing = await _unitOfWork.Coupons.GetByCodeAsync(vm.Code);
        if (existing != null)
            throw new InvalidOperationException($"兌換碼 '{vm.Code}' 已存在");

        var coupon = new Coupon
        {
            Code = vm.Code.ToUpper(),
            Name = vm.Name,
            Description = vm.Description,
            DiscountType = vm.DiscountType,
            DiscountValue = vm.DiscountValue,
            MaxDiscountAmount = vm.MaxDiscountAmount,
            ValidDays = vm.ValidDays,
            IsActive = vm.IsActive,
            IsRedeemable = vm.IsRedeemable
        };

        await _unitOfWork.Coupons.AddAsync(coupon);
        await _unitOfWork.SaveChangesAsync();

        vm.Id = coupon.Id;
        _logger.LogInformation("優惠券新增：CouponId={CouponId}, Code={Code}, Name={Name}", coupon.Id, coupon.Code, coupon.Name);
    }

    public async Task UpdateCouponAsync(CouponFormViewModel vm)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(vm.Id);
        ValidationHelper.ValidateEntityExists(coupon, "優惠券", vm.Id);

        // 檢查兌換碼唯一性（排除自己）
        var codeExisting = await _unitOfWork.Coupons.GetByCodeAsync(vm.Code);
        if (codeExisting != null && codeExisting.Id != vm.Id)
            throw new InvalidOperationException($"兌換碼 '{vm.Code}' 已被其他優惠券使用");

        coupon!.Code = vm.Code.ToUpper();
        coupon.Name = vm.Name;
        coupon.Description = vm.Description;
        coupon.DiscountType = vm.DiscountType;
        coupon.DiscountValue = vm.DiscountValue;
        coupon.MaxDiscountAmount = vm.MaxDiscountAmount;
        coupon.ValidDays = vm.ValidDays;
        coupon.IsActive = vm.IsActive;
        coupon.IsRedeemable = vm.IsRedeemable;

        await _unitOfWork.Coupons.UpdateAsync(coupon);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteCouponAsync(int id)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
        ValidationHelper.ValidateEntityExists(coupon, "優惠券", id);

        _logger.LogInformation("優惠券刪除：CouponId={CouponId}, Code={Code}", coupon!.Code, id);
        await _unitOfWork.Coupons.DeleteAsync(coupon);
        await _unitOfWork.SaveChangesAsync();
    }

    // ==================== 使用者 ====================

    public async Task<IEnumerable<UserCouponViewModel>> GetUserCouponsAsync(string userId)
    {
        ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

        var userCoupons = await _unitOfWork.Coupons.GetUserCouponsAsync(userId);
        return _mapper.Map<IEnumerable<UserCouponViewModel>>(userCoupons);
    }

    public async Task<IEnumerable<AvailableCouponViewModel>> GetAvailableCouponsForCheckoutAsync(string userId)
    {
        ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));

        var userCoupons = await _unitOfWork.Coupons.GetAvailableUserCouponsAsync(userId);
        return _mapper.Map<IEnumerable<AvailableCouponViewModel>>(userCoupons);
    }

    public async Task<CouponApplyResultViewModel> ValidateAndCalculateDiscountAsync(
        int userCouponId, string userId, decimal totalAmount)
    {
        var userCoupon = await _unitOfWork.Coupons.GetUserCouponByIdAsync(userCouponId);

        if (userCoupon == null || userCoupon.UserId != userId)
            return new CouponApplyResultViewModel { Success = false, Message = "找不到此優惠券" };

        if (userCoupon.IsUsed)
            return new CouponApplyResultViewModel { Success = false, Message = "此優惠券已使用" };

        if (userCoupon.ExpiresAt <= DateTime.UtcNow)
            return new CouponApplyResultViewModel { Success = false, Message = "此優惠券已過期" };

        var coupon = userCoupon.Coupon!;
        var discount = CalculateDiscount(coupon, totalAmount);

        return new CouponApplyResultViewModel
        {
            Success = true,
            Message = "優惠券套用成功",
            DiscountAmount = discount,
            FinalAmount = totalAmount - discount
        };
    }

    // ==================== 兌換 ====================

    public async Task<(bool Success, string Message)> RedeemCouponByCodeAsync(string userId, string code)
    {
        ValidationHelper.ValidateNotEmpty(userId, "使用者 ID", nameof(userId));
        ValidationHelper.ValidateNotEmpty(code, "兌換碼", nameof(code));

        var coupon = await _unitOfWork.Coupons.GetByCodeAsync(code.ToUpper());

        if (coupon == null)
            return (false, "無效的兌換碼");

        if (!coupon.IsActive)
            return (false, "此優惠券已停用");

        if (!coupon.IsRedeemable)
            return (false, "此優惠券不支援兌換碼兌換");

        // 檢查是否已兌換過（同一張優惠券、兌換碼來源、同年度只能兌換一次）
        var alreadyRedeemed = await _unitOfWork.Coupons
            .HasUserReceivedCouponAsync(userId, coupon.Id, CouponSource.CodeRedemption, DateTime.UtcNow.Year);
        if (alreadyRedeemed)
            return (false, "您已兌換過此優惠券");

        var userCoupon = new UserCoupon
        {
            UserId = userId,
            CouponId = coupon.Id,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(coupon.ValidDays),
            Source = CouponSource.CodeRedemption
        };

        await _unitOfWork.Coupons.AddUserCouponAsync(userCoupon);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("優惠券兌換成功：UserId={UserId}, CouponCode={Code}, CouponName={Name}", userId, code, coupon.Name);
        return (true, $"兌換成功！獲得「{coupon.Name}」，有效期 {coupon.ValidDays} 天");
    }

    // ==================== 發放 ====================

    public async Task<(bool Success, string Message)> IssueCouponToUserAsync(int couponId, string userEmail)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(couponId);
        if (coupon == null)
            return (false, "找不到此優惠券");

        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user == null)
            return (false, $"找不到 Email 為 '{userEmail}' 的使用者");

        var userCoupon = new UserCoupon
        {
            UserId = user.Id,
            CouponId = couponId,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(coupon.ValidDays),
            Source = CouponSource.AdminGrant
        };

        await _unitOfWork.Coupons.AddUserCouponAsync(userCoupon);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("優惠券發放：CouponId={CouponId}, 發放給={UserEmail}", couponId, userEmail);
        return (true, $"已成功發放「{coupon.Name}」給 {userEmail}");
    }

    public async Task<int> IssueCouponToAllUsersAsync(int couponId)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(couponId);
        if (coupon == null)
            throw new InvalidOperationException("找不到此優惠券");

        var currentYear = DateTime.UtcNow.Year;

        // 取得所有使用者
        var allUsers = _userManager.Users.ToList();

        var userCouponsToAdd = new List<UserCoupon>();

        foreach (var user in allUsers)
        {
            // 排除今年已透過統一發放領取過的使用者
            var alreadyReceived = await _unitOfWork.Coupons
                .HasUserReceivedCouponAsync(user.Id, couponId, CouponSource.AdminGrant, currentYear);

            if (alreadyReceived) continue;

            userCouponsToAdd.Add(new UserCoupon
            {
                UserId = user.Id,
                CouponId = couponId,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(coupon.ValidDays),
                Source = CouponSource.AdminGrant
            });
        }

        if (userCouponsToAdd.Count > 0)
        {
            await _unitOfWork.Coupons.AddUserCouponsRangeAsync(userCouponsToAdd);
            await _unitOfWork.SaveChangesAsync();
        }

        _logger.LogInformation("統一發放優惠券：CouponId={CouponId}, 發放人數={Count}", couponId, userCouponsToAdd.Count);
        return userCouponsToAdd.Count;
    }

    public async Task<int> IssueBirthdayCouponsAsync(int couponId)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(couponId);
        if (coupon == null)
            throw new InvalidOperationException("找不到此優惠券");

        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;

        // 查詢當月壽星
        var birthdayUsers = _userManager.Users
            .Where(u => u.Birthday.HasValue && u.Birthday.Value.Month == currentMonth)
            .ToList();

        var issuedCount = 0;

        foreach (var user in birthdayUsers)
        {
            // 排除今年已領取者
            var alreadyReceived = await _unitOfWork.Coupons
                .HasUserReceivedCouponAsync(user.Id, couponId, CouponSource.BirthdayGift, currentYear);

            if (alreadyReceived) continue;

            var userCoupon = new UserCoupon
            {
                UserId = user.Id,
                CouponId = couponId,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(coupon.ValidDays),
                Source = CouponSource.BirthdayGift
            };

            await _unitOfWork.Coupons.AddUserCouponAsync(userCoupon);
            issuedCount++;
        }

        if (issuedCount > 0)
            await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("生日優惠券發放：CouponId={CouponId}, 當月壽星={Count}", couponId, issuedCount);
        return issuedCount;
    }

    // ==================== 訂單整合 ====================

    public async Task MarkCouponAsUsedAsync(int userCouponId, int orderId)
    {
        var userCoupon = await _unitOfWork.Coupons.GetUserCouponByIdAsync(userCouponId);
        if (userCoupon == null)
            throw new InvalidOperationException("找不到此優惠券");

        userCoupon.IsUsed = true;
        userCoupon.UsedAt = DateTime.UtcNow;
        userCoupon.OrderId = orderId;

        await _unitOfWork.Coupons.UpdateUserCouponAsync(userCoupon);
    }

    public async Task ReleaseCouponAsync(int userCouponId)
    {
        var userCoupon = await _unitOfWork.Coupons.GetUserCouponByIdAsync(userCouponId);
        if (userCoupon == null) return;

        userCoupon.IsUsed = false;
        userCoupon.UsedAt = null;
        userCoupon.OrderId = null;

        await _unitOfWork.Coupons.UpdateUserCouponAsync(userCoupon);
    }

    // ==================== 私有輔助方法 ====================

    /// <summary>
    /// 計算折扣金額
    /// </summary>
    private static decimal CalculateDiscount(Coupon coupon, decimal totalAmount)
    {
        decimal discount;

        if (coupon.DiscountType == DiscountType.FixedAmount)
        {
            discount = coupon.DiscountValue;
        }
        else
        {
            // 百分比折扣：DiscountValue 代表折多少（例如 10 = 折 10%）
            discount = totalAmount * coupon.DiscountValue / 100;

            // 套用最高折扣上限
            if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
                discount = coupon.MaxDiscountAmount.Value;
        }

        // 折扣不能超過總金額
        return Math.Min(discount, totalAmount);
    }
}
