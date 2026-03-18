using Microsoft.EntityFrameworkCore;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;
using System.Linq.Expressions;

namespace MusicShop.Data.Repositories.Implementation
{
    /// <summary>
    /// 通用儲存庫實作，提供基本的 CRUD 操作。
    /// 刪除操作會自動判斷實體是否實作 ISoftDeletable：
    /// - 實作者：透過 DbContext.SaveChangesAsync 攔截器自動轉為軟刪除
    /// - 未實作者：執行硬刪除（如 CartItem、WishlistItem）
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 刪除實體。ISoftDeletable 實體會在 SaveChangesAsync 時自動轉為軟刪除。
        /// </summary>
        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 批次刪除實體。ISoftDeletable 實體會在 SaveChangesAsync 時自動轉為軟刪除。
        /// </summary>
        public virtual Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }
    }
}
