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

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // Repository 屬性（第一次存取時才建立實例，共用同一個 DbContext）
        public IAlbumRepository Albums => _albums ??= new AlbumRepository(_context);
        public IArtistRepository Artists => _artists ??= new ArtistRepository(_context);
        public IArtistCategoryRepository ArtistCategories => _artistCategories ??= new ArtistCategoryRepository(_context);
        public IProductTypeRepository ProductTypes => _productTypes ??= new ProductTypeRepository(_context);
        public ICartRepository Cart => _cart ??= new CartRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public IStatisticsRepository Statistics => _statistics ??= new StatisticsRepository(_context);
        public IBannerRepository Banners => _banners ??= new BannerRepository(_context);

        /// <summary>
        /// 開始資料庫交易
        /// </summary>
        public async Task BeginTransactionAsync()
        {
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
