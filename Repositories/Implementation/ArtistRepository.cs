using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;
using MusicShop.Repositories.Interface;

namespace MusicShop.Repositories.Implementation;

/// <summary>
/// 藝人資料存取實作
/// </summary>
public class ArtistRepository : IArtistRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ArtistRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<Artist>> GetAllArtistsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Artists
            .Include(a => a.ArtistCategory)
            .OrderBy(a => a.ArtistCategory.DisplayOrder)
            .ThenBy(a => a.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Artist>> GetArtistsByCategoryIdAsync(int artistCategoryId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Artists
            .Where(a => a.ArtistCategoryId == artistCategoryId)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Artist?> GetArtistByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Artists
            .Include(a => a.ArtistCategory)
            .Include(a => a.Albums)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Dictionary<ArtistCategory, IEnumerable<Artist>>> GetArtistsGroupedByCategoryAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // 取得所有藝人分類（包含藝人資料）
        var artistCategories = await context.ArtistCategories
            .Include(ac => ac.Artists)
            .OrderBy(ac => ac.DisplayOrder)
            .ToListAsync();

        // 建立分組字典
        var result = new Dictionary<ArtistCategory, IEnumerable<Artist>>();

        foreach (var category in artistCategories)
        {
            var artists = category.Artists
                .OrderBy(a => a.DisplayOrder)
                .ToList();
            result[category] = artists;
        }

        return result;
    }

    public async Task<Artist> AddArtistAsync(Artist artist)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        return artist;
    }

    public async Task UpdateArtistAsync(Artist artist)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Artists.Update(artist);
        await context.SaveChangesAsync();
    }

    public async Task DeleteArtistAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var artist = await context.Artists.FindAsync(id);
        if (artist != null)
        {
            context.Artists.Remove(artist);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ArtistExistsAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Artists.AnyAsync(a => a.Id == id);
    }
}
