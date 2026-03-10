using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation;

/// <summary>
/// 藝人分類資料存取實作
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
            .OrderBy(ac => ac.DisplayOrder)
            .ThenBy(ac => ac.Name)
            .ToListAsync();
    }

    public async Task<ArtistCategory?> GetByIdAsync(int id)
    {
        return await _context.ArtistCategories.FindAsync(id);
    }

    public async Task<ArtistCategory> CreateAsync(ArtistCategory artistCategory)
    {
        _context.ArtistCategories.Add(artistCategory);
        await _context.SaveChangesAsync();
        return artistCategory;
    }

    public async Task UpdateAsync(ArtistCategory artistCategory)
    {
        _context.ArtistCategories.Update(artistCategory);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var artistCategory = await _context.ArtistCategories.FindAsync(id);
        if (artistCategory != null)
        {
            _context.ArtistCategories.Remove(artistCategory);
            await _context.SaveChangesAsync();
        }
    }
}
