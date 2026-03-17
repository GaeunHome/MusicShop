using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation;

/// <summary>
/// 藝人分類資料存取實作
/// 注意：寫入操作不呼叫 SaveChangesAsync，由 UnitOfWork 統一管理。
/// </summary>
public class ArtistCategoryRepository : IArtistCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public ArtistCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ArtistCategory>> GetAllAsync()
    {
        return await _context.ArtistCategories
            .AsNoTracking()
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 根據 ID 取得藝人分類（保持追蹤以支援更新）
    /// </summary>
    public async Task<ArtistCategory?> GetByIdAsync(int id)
    {
        return await _context.ArtistCategories.FindAsync(id);
    }

    public async Task<ArtistCategory> CreateAsync(ArtistCategory artistCategory)
    {
        await _context.ArtistCategories.AddAsync(artistCategory);
        return artistCategory;
    }

    public Task UpdateAsync(ArtistCategory artistCategory)
    {
        _context.ArtistCategories.Update(artistCategory);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var targetCategory = await _context.ArtistCategories.FindAsync(id);
        if (targetCategory != null)
        {
            _context.ArtistCategories.Remove(targetCategory);
        }
    }
}
