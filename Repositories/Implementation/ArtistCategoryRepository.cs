using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;
using MusicShop.Repositories.Interface;

namespace MusicShop.Repositories.Implementation;

/// <summary>
/// 藝人分類資料存取實作
/// </summary>
public class ArtistCategoryRepository : IArtistCategoryRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ArtistCategoryRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<ArtistCategory>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ArtistCategories
            .OrderBy(ac => ac.DisplayOrder)
            .ThenBy(ac => ac.Name)
            .ToListAsync();
    }

    public async Task<ArtistCategory?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ArtistCategories.FindAsync(id);
    }

    public async Task<ArtistCategory> CreateAsync(ArtistCategory artistCategory)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.ArtistCategories.Add(artistCategory);
        await context.SaveChangesAsync();
        return artistCategory;
    }

    public async Task UpdateAsync(ArtistCategory artistCategory)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.ArtistCategories.Update(artistCategory);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var artistCategory = await context.ArtistCategories.FindAsync(id);
        if (artistCategory != null)
        {
            context.ArtistCategories.Remove(artistCategory);
            await context.SaveChangesAsync();
        }
    }
}
