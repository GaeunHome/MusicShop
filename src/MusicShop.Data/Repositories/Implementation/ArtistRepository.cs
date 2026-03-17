using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation;

/// <summary>
/// 藝人資料存取實作
/// 注意：寫入操作不呼叫 SaveChangesAsync，由 UnitOfWork 統一管理。
/// </summary>
public class ArtistRepository : IArtistRepository
{
    private readonly ApplicationDbContext _context;

    public ArtistRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 取得所有藝人（含分類和專輯數量），依分類排序後再依個別排序
    /// </summary>
    public async Task<IEnumerable<Artist>> GetAllArtistsAsync()
    {
        return await _context.Artists
            .AsNoTracking()
            .Include(artist => artist.ArtistCategory)
            .Include(artist => artist.Albums)
            .OrderBy(artist => artist.ArtistCategory.DisplayOrder)
            .ThenBy(artist => artist.DisplayOrder)
            .ToListAsync();
    }

    /// <summary>
    /// 取得指定分類下的藝人
    /// </summary>
    public async Task<IEnumerable<Artist>> GetArtistsByCategoryIdAsync(int artistCategoryId)
    {
        return await _context.Artists
            .AsNoTracking()
            .Where(artist => artist.ArtistCategoryId == artistCategoryId)
            .OrderBy(artist => artist.DisplayOrder)
            .ToListAsync();
    }

    /// <summary>
    /// 根據 ID 取得藝人（含分類與所屬專輯，保持追蹤以支援更新）
    /// </summary>
    public async Task<Artist?> GetArtistByIdAsync(int id)
    {
        return await _context.Artists
            .Include(artist => artist.ArtistCategory)
            .Include(artist => artist.Albums)
            .FirstOrDefaultAsync(artist => artist.Id == id);
    }

    /// <summary>
    /// 取得所有上架藝人並依分類分組（用於導覽列下拉選單）
    /// 僅包含 IsActive = true 的藝人，下架藝人不在前台導覽中顯示
    /// </summary>
    public async Task<Dictionary<ArtistCategory, IEnumerable<Artist>>> GetArtistsGroupedByCategoryAsync()
    {
        var allCategories = await _context.ArtistCategories
            .AsNoTracking()
            .Include(category => category.Artists)
            .OrderBy(category => category.DisplayOrder)
            .ToListAsync();

        var groupedResult = new Dictionary<ArtistCategory, IEnumerable<Artist>>();

        foreach (var category in allCategories)
        {
            var sortedArtists = category.Artists
                .Where(artist => artist.IsActive)
                .OrderBy(artist => artist.DisplayOrder)
                .ToList();
            groupedResult[category] = sortedArtists;
        }

        return groupedResult;
    }

    public async Task<Artist> AddArtistAsync(Artist artist)
    {
        await _context.Artists.AddAsync(artist);
        return artist;
    }

    public Task UpdateArtistAsync(Artist artist)
    {
        _context.Artists.Update(artist);
        return Task.CompletedTask;
    }

    public async Task DeleteArtistAsync(int id)
    {
        var targetArtist = await _context.Artists.FindAsync(id);
        if (targetArtist != null)
        {
            _context.Artists.Remove(targetArtist);
        }
    }

    public async Task<bool> ArtistExistsAsync(int id)
    {
        return await _context.Artists.AnyAsync(artist => artist.Id == id);
    }

    /// <summary>
    /// 取得目前最大的排序順序值（若無藝人則回傳 0）
    /// </summary>
    public async Task<int> GetMaxDisplayOrderAsync()
    {
        if (!await _context.Artists.AnyAsync())
            return 0;

        return await _context.Artists.MaxAsync(artist => artist.DisplayOrder);
    }

    /// <summary>
    /// 分頁取得藝人列表（含分類和專輯數量），支援依分類及上架狀態篩選
    /// </summary>
    public async Task<(IEnumerable<Artist> Items, int TotalCount)> GetArtistsPagedAsync(
        int page, int pageSize, int? artistCategoryId = null, bool? isActive = null)
    {
        var query = _context.Artists
            .AsNoTracking()
            .Include(artist => artist.ArtistCategory)
            .Include(artist => artist.Albums)
            .AsQueryable();

        if (artistCategoryId.HasValue)
            query = query.Where(a => a.ArtistCategoryId == artistCategoryId.Value);

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        query = query
            .OrderBy(a => a.ArtistCategory.DisplayOrder)
            .ThenBy(a => a.DisplayOrder);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
