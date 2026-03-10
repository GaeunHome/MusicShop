using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation;

/// <summary>
/// 商品類型資料存取實作
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
            .OrderBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Name)
            .ToListAsync();
    }

    public async Task<ProductType?> GetByIdAsync(int id)
    {
        return await _context.ProductTypes.FindAsync(id);
    }

    public async Task<ProductType> CreateAsync(ProductType productType)
    {
        _context.ProductTypes.Add(productType);
        await _context.SaveChangesAsync();
        return productType;
    }

    public async Task UpdateAsync(ProductType productType)
    {
        _context.ProductTypes.Update(productType);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var productType = await _context.ProductTypes.FindAsync(id);
        if (productType != null)
        {
            _context.ProductTypes.Remove(productType);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ProductType>> GetParentCategoriesAsync()
    {
        return await _context.ProductTypes
            .Where(pt => pt.ParentId == null)
            .OrderBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductType>> GetChildrenByParentIdAsync(int parentId)
    {
        return await _context.ProductTypes
            .Where(pt => pt.ParentId == parentId)
            .OrderBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Name)
            .ToListAsync();
    }

    public async Task<ProductType?> GetByIdWithChildrenAsync(int id)
    {
        return await _context.ProductTypes
            .Include(pt => pt.Children.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name))
            .FirstOrDefaultAsync(pt => pt.Id == id);
    }

    public async Task<ProductType?> GetByIdWithParentAsync(int id)
    {
        return await _context.ProductTypes
            .Include(pt => pt.Parent)
            .FirstOrDefaultAsync(pt => pt.Id == id);
    }

    public async Task<IEnumerable<ProductType>> GetAllChildCategoriesAsync()
    {
        return await _context.ProductTypes
            .Include(pt => pt.Parent)
            .Where(pt => pt.ParentId != null)
            .OrderBy(pt => pt.Parent!.DisplayOrder)
            .ThenBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Name)
            .ToListAsync();
    }
}
