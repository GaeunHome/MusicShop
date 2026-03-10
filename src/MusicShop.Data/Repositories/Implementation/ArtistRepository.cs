using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation;

/// <summary>
/// 藝人資料存取實作
/// </summary>
public class ArtistRepository : IArtistRepository
{
    private readonly ApplicationDbContext _context;

    public ArtistRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Artist>> GetAllArtistsAsync()
    {
        return await _context.Artists
            .Include(a => a.ArtistCategory)
            .OrderBy(a => a.ArtistCategory.DisplayOrder)
            .ThenBy(a => a.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Artist>> GetArtistsByCategoryIdAsync(int artistCategoryId)
    {
        return await _context.Artists
            .Where(a => a.ArtistCategoryId == artistCategoryId)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Artist?> GetArtistByIdAsync(int id)
    {
        return await _context.Artists
            .Include(a => a.ArtistCategory)
            .Include(a => a.Albums)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Dictionary<ArtistCategory, IEnumerable<Artist>>> GetArtistsGroupedByCategoryAsync()
    {
        // 取得所有藝人分類（包含藝人資料）
        var artistCategories = await _context.ArtistCategories
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
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();
        return artist;
    }

    public async Task UpdateArtistAsync(Artist artist)
    {
        _context.Artists.Update(artist);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteArtistAsync(int id)
    {
        var artist = await _context.Artists.FindAsync(id);
        if (artist != null)
        {
            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ArtistExistsAsync(int id)
    {
        return await _context.Artists.AnyAsync(a => a.Id == id);
    }
}
