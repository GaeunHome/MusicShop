using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation;

/// <summary>
/// 商品類型資料存取實作（支援階層式父子分類）
/// 注意：寫入操作不呼叫 SaveChangesAsync，由 UnitOfWork 統一管理。
/// </summary>
public class ProductTypeRepository : IProductTypeRepository
{
    private readonly ApplicationDbContext _context;

    public ProductTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductType>> GetAllAsync()
    {
        return await _context.ProductTypes
            .AsNoTracking()
            .OrderBy(type => type.DisplayOrder)
            .ThenBy(type => type.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 根據 ID 取得商品類型（保持追蹤以支援更新）
    /// </summary>
    public async Task<ProductType?> GetByIdAsync(int id)
    {
        return await _context.ProductTypes.FindAsync(id);
    }

    public async Task<ProductType> CreateAsync(ProductType productType)
    {
        await _context.ProductTypes.AddAsync(productType);
        return productType;
    }

    public Task UpdateAsync(ProductType productType)
    {
        _context.ProductTypes.Update(productType);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var targetType = await _context.ProductTypes.FindAsync(id);
        if (targetType != null)
        {
            _context.ProductTypes.Remove(targetType);
        }
    }

    /// <summary>
    /// 取得所有頂層父分類（ParentId == null）
    /// </summary>
    public async Task<IEnumerable<ProductType>> GetParentCategoriesAsync()
    {
        return await _context.ProductTypes
            .AsNoTracking()
            .Where(type => type.ParentId == null)
            .OrderBy(type => type.DisplayOrder)
            .ThenBy(type => type.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 取得指定父分類下的所有子分類
    /// </summary>
    public async Task<IEnumerable<ProductType>> GetChildrenByParentIdAsync(int parentId)
    {
        return await _context.ProductTypes
            .AsNoTracking()
            .Where(type => type.ParentId == parentId)
            .OrderBy(type => type.DisplayOrder)
            .ThenBy(type => type.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 根據 ID 取得商品類型（含子分類，保持追蹤用於刪除檢查）
    /// </summary>
    public async Task<ProductType?> GetByIdWithChildrenAsync(int id)
    {
        return await _context.ProductTypes
            .Include(type => type.Children.OrderBy(child => child.DisplayOrder).ThenBy(child => child.Name))
            .FirstOrDefaultAsync(type => type.Id == id);
    }

    /// <summary>
    /// 根據 ID 取得商品類型（含父分類資訊）
    /// </summary>
    public async Task<ProductType?> GetByIdWithParentAsync(int id)
    {
        return await _context.ProductTypes
            .Include(type => type.Parent)
            .FirstOrDefaultAsync(type => type.Id == id);
    }

    public async Task<bool> HasChildrenAsync(int id)
    {
        return await _context.ProductTypes.AnyAsync(type => type.ParentId == id);
    }

    /// <summary>
    /// 取得所有父分類（含子分類），單次查詢避免 N+1 問題
    /// </summary>
    public async Task<IEnumerable<ProductType>> GetParentCategoriesWithChildrenAsync()
    {
        return await _context.ProductTypes
            .AsNoTracking()
            .Where(type => type.ParentId == null)
            .Include(type => type.Children.OrderBy(child => child.DisplayOrder).ThenBy(child => child.Name))
            .OrderBy(type => type.DisplayOrder)
            .ThenBy(type => type.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 取得所有子分類（含父分類資訊，用於下拉選單）
    /// </summary>
    public async Task<IEnumerable<ProductType>> GetAllChildCategoriesAsync()
    {
        return await _context.ProductTypes
            .AsNoTracking()
            .Include(type => type.Parent)
            .Where(type => type.ParentId != null)
            // Parent! 的 null-forgiving 是安全的：上方 Where 已篩選 ParentId != null，
            // 且 Include(Parent) 確保導航屬性已載入，此處不會為 null。
            .OrderBy(type => type.Parent!.DisplayOrder)
            .ThenBy(type => type.DisplayOrder)
            .ThenBy(type => type.Name)
            .ToListAsync();
    }
}
