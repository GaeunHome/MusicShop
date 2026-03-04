using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;
using MusicShop.Repositories.Interface;

namespace MusicShop.Repositories.Implementation
{
    /// <summary>
    /// 專輯資料存取實作
    /// </summary>
    public class AlbumRepository : IAlbumRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AlbumRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Album>> GetAllAlbumsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Albums
                .Include(a => a.ArtistCategory)
                .Include(a => a.ProductType)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Album>> GetAlbumsAsync(
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Albums
                .Include(a => a.ArtistCategory)
                .Include(a => a.ProductType)
                .AsQueryable();

            // 搜尋關鍵字
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a =>
                    a.Title.Contains(searchTerm) ||
                    a.Artist.Contains(searchTerm));
            }

            // 藝人分類篩選
            if (artistCategoryId.HasValue)
            {
                query = query.Where(a => a.ArtistCategoryId == artistCategoryId);
            }

            // 商品類型篩選（子分類）
            if (productTypeId.HasValue)
            {
                query = query.Where(a => a.ProductTypeId == productTypeId);
            }
            // 商品父分類篩選（顯示該父分類下所有子分類的商品）
            else if (parentProductTypeId.HasValue)
            {
                // 查詢該父分類下所有子分類的 ID
                var childCategoryIds = await context.ProductTypes
                    .Where(pt => pt.ParentId == parentProductTypeId)
                    .Select(pt => pt.Id)
                    .ToListAsync();

                if (childCategoryIds.Any())
                {
                    query = query.Where(a => a.ProductTypeId.HasValue && childCategoryIds.Contains(a.ProductTypeId.Value));
                }
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Album?> GetAlbumByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Albums
                .Include(a => a.ArtistCategory)
                .Include(a => a.ProductType)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int count)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Albums
                .Include(a => a.ArtistCategory)
                .Include(a => a.ProductType)
                .OrderByDescending(a => a.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Album> AddAlbumAsync(Album album)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Albums.Add(album);
            await context.SaveChangesAsync();
            return album;
        }

        public async Task UpdateAlbumAsync(Album album)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Albums.Update(album);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAlbumAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var album = await context.Albums.FindAsync(id);
            if (album != null)
            {
                context.Albums.Remove(album);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> AlbumExistsAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Albums.AnyAsync(a => a.Id == id);
        }
    }
}
