using MusicShop.Models;
using MusicShop.Repositories.Interface;
using MusicShop.Services.Interface;

namespace MusicShop.Services.Implementation;

/// <summary>
/// 商品類型商業邏輯實作
/// </summary>
public class ProductTypeService : IProductTypeService
{
    private readonly IProductTypeRepository _productTypeRepository;
    private readonly IAlbumRepository _albumRepository;

    public ProductTypeService(
        IProductTypeRepository productTypeRepository,
        IAlbumRepository albumRepository)
    {
        _productTypeRepository = productTypeRepository;
        _albumRepository = albumRepository;
    }

    public async Task<IEnumerable<ProductType>> GetAllProductTypesAsync()
    {
        return await _productTypeRepository.GetAllAsync();
    }

    public async Task<ProductType?> GetProductTypeByIdAsync(int id)
    {
        return await _productTypeRepository.GetByIdAsync(id);
    }

    public async Task<ProductType> CreateProductTypeAsync(ProductType productType)
    {
        // 商業邏輯驗證
        if (string.IsNullOrWhiteSpace(productType.Name))
        {
            throw new ArgumentException("商品類型名稱不能為空", nameof(productType.Name));
        }

        if (productType.Name.Length > 50)
        {
            throw new ArgumentException("商品類型名稱不能超過 50 個字元", nameof(productType.Name));
        }

        return await _productTypeRepository.CreateAsync(productType);
    }

    public async Task UpdateProductTypeAsync(ProductType productType)
    {
        // 商業邏輯驗證
        if (string.IsNullOrWhiteSpace(productType.Name))
        {
            throw new ArgumentException("商品類型名稱不能為空", nameof(productType.Name));
        }

        if (productType.Name.Length > 50)
        {
            throw new ArgumentException("商品類型名稱不能超過 50 個字元", nameof(productType.Name));
        }

        var exists = await _productTypeRepository.GetByIdAsync(productType.Id);
        if (exists == null)
        {
            throw new InvalidOperationException($"找不到 ID 為 {productType.Id} 的商品類型");
        }

        await _productTypeRepository.UpdateAsync(productType);
    }

    public async Task DeleteProductTypeAsync(int id)
    {
        var exists = await _productTypeRepository.GetByIdWithChildrenAsync(id);
        if (exists == null)
        {
            throw new InvalidOperationException($"找不到 ID 為 {id} 的商品類型");
        }

        // 檢查是否有子分類
        if (exists.Children.Any())
        {
            throw new InvalidOperationException($"無法刪除「{exists.Name}」，因為還有 {exists.Children.Count} 個子分類");
        }

        // 檢查是否有商品使用此類型
        var albums = await _albumRepository.GetAlbumsAsync(null, null, id);
        if (albums.Any())
        {
            throw new InvalidOperationException($"無法刪除「{exists.Name}」，因為還有 {albums.Count()} 個商品使用此類型");
        }

        await _productTypeRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<ProductType>> GetParentCategoriesAsync()
    {
        return await _productTypeRepository.GetParentCategoriesAsync();
    }

    public async Task<IEnumerable<ProductType>> GetChildrenByParentIdAsync(int parentId)
    {
        return await _productTypeRepository.GetChildrenByParentIdAsync(parentId);
    }

    public async Task<ProductType?> GetProductTypeWithChildrenAsync(int id)
    {
        return await _productTypeRepository.GetByIdWithChildrenAsync(id);
    }

    public async Task<IEnumerable<ProductType>> GetAllChildCategoriesAsync()
    {
        return await _productTypeRepository.GetAllChildCategoriesAsync();
    }
}
