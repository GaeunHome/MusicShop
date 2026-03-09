using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;
using MusicShop.Repositories.Interface;

namespace MusicShop.Repositories.Implementation;

/// <summary>
/// 商品類型資料存取實作
/// </summary>
public class ProductTypeRepository : IProductTypeRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ProductTypeRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<ProductType>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductTypes
            .OrderBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Name)
            .ToListAsync();
    }

    public async Task<ProductType?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductTypes.FindAsync(id);
    }

    public async Task<ProductType> CreateAsync(ProductType productType)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.ProductTypes.Add(productType);
        await context.SaveChangesAsync();
        return productType;
    }

    public async Task UpdateAsync(ProductType productType)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.ProductTypes.Update(productType);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var productType = await context.ProductTypes.FindAsync(id);
        if (productType != null)
        {
            context.ProductTypes.Remove(productType);
            await context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ProductType>> GetParentCategoriesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductTypes
            .Where(pt => pt.ParentId == null)
            .OrderBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductType>> GetChildrenByParentIdAsync(int parentId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductTypes
            .Where(pt => pt.ParentId == parentId)
            .OrderBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Name)
            .ToListAsync();
    }

    public async Task<ProductType?> GetByIdWithChildrenAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductTypes
            .Include(pt => pt.Children.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name))
            .FirstOrDefaultAsync(pt => pt.Id == id);
    }

    public async Task<ProductType?> GetByIdWithParentAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductTypes
            .Include(pt => pt.Parent)
            .FirstOrDefaultAsync(pt => pt.Id == id);
    }

    public async Task<IEnumerable<ProductType>> GetAllChildCategoriesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductTypes
            .Include(pt => pt.Parent)
            .Where(pt => pt.ParentId != null)
            .OrderBy(pt => pt.Parent!.DisplayOrder)
            .ThenBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Name)
            .ToListAsync();
    }
}
