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
        private readonly ApplicationDbContext _context;

        public AlbumRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Album>> GetAllAlbumsAsync()
        {
            return await _context.Albums
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Album>> GetAlbumsAsync(string? searchTerm = null, int? categoryId = null)
        {
            var query = _context.Albums
                .Include(a => a.Category)
                .AsQueryable();

            // 搜尋關鍵字
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a =>
                    a.Title.Contains(searchTerm) ||
                    a.Artist.Contains(searchTerm));
            }

            // 分類篩選
            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId);
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Album?> GetAlbumByIdAsync(int id)
        {
            return await _context.Albums
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int count)
        {
            return await _context.Albums
                .Include(a => a.Category)
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
