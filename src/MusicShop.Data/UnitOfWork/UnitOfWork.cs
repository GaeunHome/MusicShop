using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MusicShop.Data.Repositories.Interfaces;
using MusicShop.Data.Repositories.Implementation;

namespace MusicShop.Data.UnitOfWork
{
    /// <summary>
    /// Unit of Work 實作，管理所有 Repository 和資料庫交易
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repository 實例（延遲載入）
        private IAlbumRepository? _albums;
        private IArtistRepository? _artists;
        private IArtistCategoryRepository? _artistCategories;
        private IProductTypeRepository? _productTypes;
        private ICartRepository? _cart;
        private IOrderRepository? _orders;
        private IStatisticsRepository? _statistics;
        private IBannerRepository? _banners;
        private IWishlistRepository? _wishlists;
        private IFeaturedArtistRepository? _featuredArtists;
        private ICouponRepository? _coupons;
        private IPasswordHistoryRepository? _passwordHistories;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // 延遲初始化（??=）：Repository 在首次存取時才建立，且所有 Repository 共用
        // 同一個 _context 實例，確保同一個 Request 內的多個 Repository 操作
        // 都在同一個 DbContext / ChangeTracker 中執行，SaveChangesAsync 才能一次提交。
        public IAlbumRepository Albums => _albums ??= new AlbumRepository(_context);
        public IArtistRepository Artists => _artists ??= new ArtistRepository(_context);
        public IArtistCategoryRepository ArtistCategories => _artistCategories ??= new ArtistCategoryRepository(_context);
        public IProductTypeRepository ProductTypes => _productTypes ??= new ProductTypeRepository(_context);
        public ICartRepository Cart => _cart ??= new CartRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public IStatisticsRepository Statistics => _statistics ??= new StatisticsRepository(_context);
        public IBannerRepository Banners => _banners ??= new BannerRepository(_context);
        public IWishlistRepository Wishlists => _wishlists ??= new WishlistRepository(_context);
        public IFeaturedArtistRepository FeaturedArtists => _featuredArtists ??= new FeaturedArtistRepository(_context);
        public ICouponRepository Coupons => _coupons ??= new CouponRepository(_context);
        public IPasswordHistoryRepository PasswordHistories => _passwordHistories ??= new PasswordHistoryRepository(_context);

        /// <summary>
        /// 開始資料庫交易（同一時間僅支援一個活躍交易）
        /// </summary>
        /// <exception cref="InvalidOperationException">已有進行中的交易時拋出</exception>
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException(
                    "已有進行中的交易，請先呼叫 CommitAsync 或 RollbackAsync 完成當前交易。");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// 提交交易並儲存變更
        /// </summary>
        public async Task CommitAsync()
        {
            try
            {
                await SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 回滾交易
        /// </summary>
        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// 儲存所有變更到資料庫
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            // 注意：DbContext 由 DI 容器管理，不需要手動 Dispose
        }
    }
}
