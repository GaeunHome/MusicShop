namespace MusicShop.Library.Enums;

/// <summary>
/// 優惠券來源
/// </summary>
public enum CouponSource
{
    /// <summary>
    /// 管理員手動發放
    /// </summary>
    AdminGrant = 1,

    /// <summary>
    /// 使用者兌換碼兌換
    /// </summary>
    CodeRedemption = 2,

    /// <summary>
    /// 生日自動發放
    /// </summary>
    BirthdayGift = 3
}
