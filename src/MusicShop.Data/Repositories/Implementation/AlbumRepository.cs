using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation
{
    /// <summary>
    /// 專輯資料存取實作
    /// </summary>
    public class AlbumRepository : IAlbumRepository
    {
        private readonly ApplicationDbContext _context;

        public AlbumRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Album>> GetAllAlbumsAsync()
        {
            return await _context.Albums
                .Include(a => a.Artist)
                    .ThenInclude(ar => ar.ArtistCategory)
                .Include(a => a.ProductType)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Album>> GetAlbumsAsync(
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? artistId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null,
            string? sortBy = null)
        {
            var query = _context.Albums
                .Include(a => a.Artist)
                    .ThenInclude(ar => ar.ArtistCategory)
                .Include(a => a.ProductType)
                .AsQueryable();

            // 搜尋關鍵字
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a =>
                    a.Title.Contains(searchTerm) ||
                    (a.Artist != null && a.Artist.Name.Contains(searchTerm)));
            }

            // 藝人分類篩選（透過 Artist 間接取得）
            if (artistCategoryId.HasValue)
            {
                query = query.Where(a => a.Artist != null && a.Artist.ArtistCategoryId == artistCategoryId);
            }

            // 藝人/團體篩選
            if (artistId.HasValue)
            {
                query = query.Where(a => a.ArtistId == artistId);
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
                var childCategoryIds = await _context.ProductTypes
                    .Where(pt => pt.ParentId == parentProductTypeId)
                    .Select(pt => pt.Id)
                    .ToListAsync();

                if (childCategoryIds.Any())
                {
                    query = query.Where(a => a.ProductTypeId.HasValue && childCategoryIds.Contains(a.ProductTypeId.Value));
                }
            }

            // 排序邏輯
            query = sortBy switch
            {
                "price-high-low" => query.OrderByDescending(a => a.Price),
                "price-low-high" => query.OrderBy(a => a.Price),
                "date-old-new" => query.OrderBy(a => a.CreatedAt),
                "date-new-old" => query.OrderByDescending(a => a.CreatedAt),
                "weekly-hot" => query.OrderByDescending(a => a.Id), // 暫時使用 Id，未來可改為銷售量
                _ => query.OrderByDescending(a => a.CreatedAt) // 預設：最新上架
            };

            return await query.ToListAsync();
        }

        public async Task<Album?> GetAlbumByIdAsync(int id)
        {
            return await _context.Albums
                .Include(a => a.Artist)
                    .ThenInclude(ar => ar.ArtistCategory)
                .Include(a => a.ProductType)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int count)
        {
            return await _context.Albums
                .Include(a => a.Artist)
                    .ThenInclude(ar => ar.ArtistCategory)
                .Include(a => a.ProductType)
                .OrderByDescending(a => a.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Album> AddAlbumAsync(Album album)
        {
            _context.Albums.Add(album);
            await _context.SaveChangesAsync();
            return album;
        }

        public async Task UpdateAlbumAsync(Album album)
        {
            _context.Albums.Update(album);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAlbumAsync(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album != null)
            {
                _context.Albums.Remove(album);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> AlbumExistsAsync(int id)
        {
            return await _context.Albums.AnyAsync(a => a.Id == id);
        }
    }
}
