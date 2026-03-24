using Microsoft.EntityFrameworkCore;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation;

/// <summary>
/// 密碼歷史記錄資料存取實作
/// </summary>
public class PasswordHistoryRepository : IPasswordHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PasswordHistory>> GetRecentByUserIdAsync(string userId, int count)
    {
        return await _context.PasswordHistories
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task AddAsync(PasswordHistory history)
    {
        await _context.PasswordHistories.AddAsync(history);
    }

    public async Task RemoveOldRecordsAsync(string userId, int keepCount)
    {
        var oldRecords = await _context.PasswordHistories
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .Skip(keepCount)
            .ToListAsync();

        if (oldRecords.Count > 0)
        {
            _context.PasswordHistories.RemoveRange(oldRecords);
        }
    }
}
