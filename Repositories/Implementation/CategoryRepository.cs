using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;
using MusicShop.Repositories.Interface;

namespace MusicShop.Repositories.Implementation
{
    /// <summary>
    /// 分類資料存取實作
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }
    }
}
