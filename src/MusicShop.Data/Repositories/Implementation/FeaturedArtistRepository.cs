using Microsoft.EntityFrameworkCore;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation;

/// <summary>
/// 精選藝人資料存取實作
/// </summary>
public class FeaturedArtistRepository : GenericRepository<FeaturedArtist>, IFeaturedArtistRepository
{
    public FeaturedArtistRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<FeaturedArtist>> GetActiveFeaturedArtistsAsync()
    {
        return await _context.FeaturedArtists
            .AsNoTracking()
            .Where(fa => fa.IsActive)
            .Include(fa => fa.Artist)
                .ThenInclude(a => a.Albums)
            .OrderBy(fa => fa.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<FeaturedArtist>> GetAllOrderedAsync()
    {
        return await _context.FeaturedArtists
            .AsNoTracking()
            .Include(fa => fa.Artist)
                .ThenInclude(a => a.Albums)
            .OrderBy(fa => fa.DisplayOrder)
            .ToListAsync();
    }

}
