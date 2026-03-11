using MusicShop.Data.Entities;

namespace MusicShop.Data.Repositories.Interfaces
{
    public interface IBannerRepository : IGenericRepository<Banner>
    {
        /// <summary>
        /// 取得所有啟用中的幻燈片（依 DisplayOrder 排序）
        /// </summary>
        Task<IEnumerable<Banner>> GetActiveBannersAsync();

        /// <summary>
        /// 取得所有幻燈片（含關聯商品，依 DisplayOrder 排序）
        /// </summary>
        Task<IEnumerable<Banner>> GetAllOrderedAsync();
    }
}
