using MusicShop.Models;

namespace MusicShop.Repositories.Interface
{
    /// <summary>
    /// 分類資料存取介面
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// 取得所有分類
        /// </summary>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// 根據 ID 取得分類
        /// </summary>
        Task<Category?> GetCategoryByIdAsync(int id);
    }
}
