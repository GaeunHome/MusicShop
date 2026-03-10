using System.Linq.Expressions;

namespace MusicShop.Data.Repositories.Interfaces
{
    /// <summary>
    /// 通用儲存庫介面，提供基本的 CRUD 操作
    /// 所有具體的 Repository 介面都應繼承此介面
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// 根據 ID 取得單一實體
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// 取得所有實體
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// 根據條件查詢實體
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 根據條件取得單一實體
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 新增實體
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// 批次新增實體
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// 更新實體
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// 刪除實體
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// 批次刪除實體
        /// </summary>
        Task DeleteRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// 檢查實體是否存在
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 計算符合條件的實體數量
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}
