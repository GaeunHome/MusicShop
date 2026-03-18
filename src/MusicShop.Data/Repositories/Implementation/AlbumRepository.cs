using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation
{
    /// <summary>
    /// 專輯資料存取實作
    /// 注意：寫入操作（Add/Update/Delete）不呼叫 SaveChangesAsync，
    /// 統一由 Service 層透過 UnitOfWork.SaveChangesAsync() 控制儲存時機，
    /// 以支援交易管理與批次操作的原子性。
    /// </summary>
    public class AlbumRepository : IAlbumRepository
    {
        private readonly ApplicationDbContext _context;

        public AlbumRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有專輯（含藝人與商品類型），依建立時間倒序
        /// </summary>
        public async Task<IEnumerable<Album>> GetAllAlbumsAsync()
        {
            return await _context.Albums
                .AsNoTracking()
                .Include(album => album.Artist)
                    .ThenInclude(artist => artist!.ArtistCategory)
                .Include(album => album.ProductType)
                .OrderByDescending(album => album.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 依條件篩選專輯（搜尋、分類、排序）
        /// </summary>
        public async Task<IEnumerable<Album>> GetAlbumsAsync(
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? artistId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null,
            string? sortBy = null,
            int? excludeId = null)
        {
            var query = await BuildFilteredQueryAsync(searchTerm, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy, excludeId);
            return await query.ToListAsync();
        }

        /// <summary>
        /// 依條件篩選專輯（支援分頁）
        /// 先計算總筆數，再套用 Skip/Take 取得該頁資料
        /// </summary>
        public async Task<(IEnumerable<Album> Items, int TotalCount)> GetAlbumsPagedAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? artistId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null,
            string? sortBy = null,
            int? excludeId = null)
        {
            var query = await BuildFilteredQueryAsync(searchTerm, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy, excludeId);

            // 先計算符合條件的總筆數（用於分頁計算）
            var totalCount = await query.CountAsync();

            // 套用分頁：Skip 跳過前面頁數的資料，Take 取得當頁資料
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// 建立篩選與排序後的查詢（共用邏輯，供 GetAlbumsAsync 與 GetAlbumsPagedAsync 使用）
        /// </summary>
        private async Task<IQueryable<Album>> BuildFilteredQueryAsync(
            string? searchTerm,
            int? artistCategoryId,
            int? artistId,
            int? productTypeId,
            int? parentProductTypeId,
            string? sortBy,
            int? excludeId)
        {
            var query = _context.Albums
                .AsNoTracking()
                .Include(album => album.Artist)
                    .ThenInclude(artist => artist!.ArtistCategory)
                .Include(album => album.ProductType)
                .AsQueryable();

            // 排除指定 ID 的專輯（用於「相關商品」排除自身）
            if (excludeId.HasValue)
            {
                query = query.Where(album => album.Id != excludeId.Value);
            }

            // 關鍵字搜尋（標題或藝人名稱）
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(album =>
                    album.Title.Contains(searchTerm) ||
                    (album.Artist != null && album.Artist.Name.Contains(searchTerm)));
            }

            // 藝人分類篩選（透過 Artist 間接取得）
            if (artistCategoryId.HasValue)
            {
                query = query.Where(album => album.Artist != null && album.Artist.ArtistCategoryId == artistCategoryId);
            }

            // 藝人/團體篩選
            if (artistId.HasValue)
            {
                query = query.Where(album => album.ArtistId == artistId);
            }

            // 子分類篩選優先於父分類：若同時傳入 productTypeId 與 parentProductTypeId，
            // 以子分類為準（else if），因為子分類已是更精確的篩選條件。
            if (productTypeId.HasValue)
            {
                query = query.Where(album => album.ProductTypeId == productTypeId);
            }
            // 父分類篩選需額外查一次 DB 取得所有子分類 ID，再以 Contains 展開為 WHERE IN。
            // 目前商品類型數量有限（約十幾筆），效能影響不大；
            // 若子分類數量大幅增長，可考慮改用子查詢或快取。
            else if (parentProductTypeId.HasValue)
            {
                var childCategoryIds = await _context.ProductTypes
                    .Where(pt => pt.ParentId == parentProductTypeId)
                    .Select(pt => pt.Id)
                    .ToListAsync();

                if (childCategoryIds.Any())
                {
                    query = query.Where(album => album.ProductTypeId.HasValue && childCategoryIds.Contains(album.ProductTypeId.Value));
                }
            }

            // 排序邏輯
            if (sortBy == "weekly-hot")
            {
                // 本週熱賣：依本週（過去 7 天）內未取消訂單的銷售數量降序排列，
                // 無銷量的專輯排在最後，再依建立時間降序作為次要排序
                var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

                query = query
                    .GroupJoin(
                        _context.OrderItems
                            .Where(oi => oi.Order != null
                                && oi.Order.OrderDate >= oneWeekAgo
                                && oi.Order.Status != Library.Enums.OrderStatus.Cancelled),
                        album => album.Id,
                        oi => oi.AlbumId,
                        (album, orderItems) => new { Album = album, WeeklySales = orderItems.Sum(oi => oi.Quantity) })
                    .Where(x => x.WeeklySales > 0)
                    .OrderByDescending(x => x.WeeklySales)
                    .ThenByDescending(x => x.Album.CreatedAt)
                    .Select(x => x.Album);
            }
            else
            {
                query = sortBy switch
                {
                    "price-high-low" => query.OrderByDescending(album => album.Price),
                    "price-low-high" => query.OrderBy(album => album.Price),
                    "date-old-new" => query.OrderBy(album => album.CreatedAt),
                    "date-new-old" => query.OrderByDescending(album => album.CreatedAt),
                    _ => query.OrderByDescending(album => album.CreatedAt) // 預設：最新上架
                };
            }

            return query;
        }

        /// <summary>
        /// 根據 ID 取得單一專輯（含關聯資料，保持追蹤以支援後續更新）
        /// </summary>
        public async Task<Album?> GetAlbumByIdAsync(int id)
        {
            return await _context.Albums
                .Include(album => album.Artist)
                    .ThenInclude(artist => artist!.ArtistCategory)
                .Include(album => album.ProductType)
                .FirstOrDefaultAsync(album => album.Id == id);
        }

        /// <summary>
        /// 取得最新上架的專輯（首頁展示用）
        /// </summary>
        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int count)
        {
            return await _context.Albums
                .AsNoTracking()
                .Include(album => album.Artist)
                    .ThenInclude(artist => artist!.ArtistCategory)
                .Include(album => album.ProductType)
                .OrderByDescending(album => album.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Album> AddAlbumAsync(Album album)
        {
            await _context.Albums.AddAsync(album);
            return album;
        }

        public Task UpdateAlbumAsync(Album album)
        {
            // 當 Service 層先透過 GetAlbumByIdAsync（有追蹤）取得實體 A，
            // 再用前端傳入的資料建構新實體 B 呼叫 Update 時，
            // ChangeTracker 會同時存在兩個相同 Id 的 Album 實例，導致
            // InvalidOperationException: "another instance with the same key is already being tracked"。
            // 此處先將舊實例解除追蹤，確保 Update 能正常附加新實體。
            var conflictingEntry = _context.ChangeTracker.Entries<Album>()
                .FirstOrDefault(entry => entry.Entity.Id == album.Id && !ReferenceEquals(entry.Entity, album));
            if (conflictingEntry != null)
                conflictingEntry.State = EntityState.Detached;

            _context.Albums.Update(album);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 刪除專輯（軟刪除）。
        /// 呼叫 Remove 後，SaveChangesAsync 會攔截並轉為軟刪除（設定 IsDeleted = true）。
        /// </summary>
        public async Task DeleteAlbumAsync(int id)
        {
            var targetAlbum = await _context.Albums.FindAsync(id);
            if (targetAlbum != null)
            {
                _context.Albums.Remove(targetAlbum);
            }
        }

        public async Task<bool> AlbumExistsAsync(int id)
        {
            return await _context.Albums.AnyAsync(album => album.Id == id);
        }

        /// <summary>
        /// 計算指定藝人分類下的專輯數量（用於刪除分類前的檢查）
        /// </summary>
        public async Task<int> CountByArtistCategoryAsync(int artistCategoryId)
        {
            return await _context.Albums
                .CountAsync(album => album.Artist != null && album.Artist.ArtistCategoryId == artistCategoryId);
        }

        /// <summary>
        /// 計算指定商品類型下的專輯數量（用於刪除類型前的檢查）
        /// </summary>
        public async Task<int> CountByProductTypeAsync(int productTypeId)
        {
            return await _context.Albums
                .CountAsync(album => album.ProductTypeId == productTypeId);
        }
    }
}
