using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.UnitOfWork
{
    /// <summary>
    /// Unit of Work 介面，管理多個 Repository 的交易
    /// 確保多個操作在同一個資料庫交易中執行
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repository 屬性
        IAlbumRepository Albums { get; }
        IArtistRepository Artists { get; }
        IArtistCategoryRepository ArtistCategories { get; }
        IProductTypeRepository ProductTypes { get; }
        ICartRepository Cart { get; }
        IOrderRepository Orders { get; }
        IStatisticsRepository Statistics { get; }
        IBannerRepository Banners { get; }

        /// <summary>
        /// 開始資料庫交易
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// 提交交易（確認所有變更）
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// 回滾交易（取消所有變更）
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// 儲存變更到資料庫
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
