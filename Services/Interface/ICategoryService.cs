using MusicShop.Models;

namespace MusicShop.Services.Interface
{
    /// <summary>
    /// 分類商業邏輯介面
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// 取得所有分類
        /// </summary>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// 取得分類詳細資訊
        /// </summary>
        Task<Category?> GetCategoryByIdAsync(int id);
    }
}
