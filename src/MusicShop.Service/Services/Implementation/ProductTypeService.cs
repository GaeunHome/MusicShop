using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Library.Helpers;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 商品類型商業邏輯實作
/// </summary>
public class ProductTypeService : IProductTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductType>> GetAllProductTypesAsync()
    {
        return await _unitOfWork.ProductTypes.GetAllAsync();
    }

    public async Task<ProductType?> GetProductTypeByIdAsync(int id)
    {
        return await _unitOfWork.ProductTypes.GetByIdAsync(id);
    }

    public async Task<ProductType> CreateProductTypeAsync(ProductType productType)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateString(productType.Name, "商品類型名稱", 50, nameof(productType.Name));

        return await _unitOfWork.ProductTypes.CreateAsync(productType);
    }

    public async Task UpdateProductTypeAsync(ProductType productType)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateString(productType.Name, "商品類型名稱", 50, nameof(productType.Name));

        var exists = await _unitOfWork.ProductTypes.GetByIdAsync(productType.Id);
        ValidationHelper.ValidateEntityExists(exists, "商品類型", productType.Id);

        await _unitOfWork.ProductTypes.UpdateAsync(productType);
    }

    public async Task DeleteProductTypeAsync(int id)
    {
        var exists = await _unitOfWork.ProductTypes.GetByIdWithChildrenAsync(id);
        ValidationHelper.ValidateEntityExists(exists, "商品類型", id);

        // 檢查是否有子分類
        ValidationHelper.ValidateCondition(
            !exists!.Children.Any(),
            $"無法刪除「{exists.Name}」，因為還有 {exists.Children.Count} 個子分類"
        );

        // 檢查是否有商品使用此類型
        var albums = await _unitOfWork.Albums.GetAlbumsAsync(null, null, null, id);
        ValidationHelper.ValidateCondition(
            !albums.Any(),
            $"無法刪除「{exists.Name}」，因為還有 {albums.Count()} 個商品使用此類型"
        );

        await _unitOfWork.ProductTypes.DeleteAsync(id);
    }

    public async Task<IEnumerable<ProductType>> GetParentCategoriesAsync()
    {
        return await _unitOfWork.ProductTypes.GetParentCategoriesAsync();
    }

    public async Task<IEnumerable<ProductType>> GetChildrenByParentIdAsync(int parentId)
    {
        return await _unitOfWork.ProductTypes.GetChildrenByParentIdAsync(parentId);
    }

    public async Task<ProductType?> GetProductTypeWithChildrenAsync(int id)
    {
        return await _unitOfWork.ProductTypes.GetByIdWithChildrenAsync(id);
    }

    public async Task<IEnumerable<ProductType>> GetAllChildCategoriesAsync()
    {
        return await _unitOfWork.ProductTypes.GetAllChildCategoriesAsync();
    }
}
