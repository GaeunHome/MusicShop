using MusicShop.Data.Entities;

namespace MusicShop.Data.Repositories.Interfaces;

/// <summary>
/// 密碼歷史記錄資料存取介面
/// </summary>
public interface IPasswordHistoryRepository
{
    /// <summary>
    /// 取得使用者最近的密碼歷史記錄
    /// </summary>
    Task<List<PasswordHistory>> GetRecentByUserIdAsync(string userId, int count);

    /// <summary>
    /// 新增密碼歷史記錄
    /// </summary>
    Task AddAsync(PasswordHistory history);

    /// <summary>
    /// 刪除超出保留筆數的舊記錄
    /// </summary>
    Task RemoveOldRecordsAsync(string userId, int keepCount);
}
