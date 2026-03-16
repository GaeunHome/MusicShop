using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels;
using MusicShop.Service.ViewModels.Admin;

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

    public async Task<ProductTypeFormViewModel?> GetProductTypeFormByIdAsync(int id)
    {
        var entity = await _unitOfWork.ProductTypes.GetByIdAsync(id);
        if (entity == null) return null;

        return new ProductTypeFormViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ParentId = entity.ParentId,
            DisplayOrder = entity.DisplayOrder
        };
    }

    public async Task<ProductTypeFormViewModel> CreateProductTypeAsync(ProductTypeFormViewModel vm)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateString(vm.Name, "商品類型名稱", 50, nameof(vm.Name));

        var entity = new ProductType
        {
            Name = vm.Name,
            Description = vm.Description,
            ParentId = vm.ParentId,
            DisplayOrder = vm.DisplayOrder
        };

        var created = await _unitOfWork.ProductTypes.CreateAsync(entity);
        vm.Id = created.Id;
        return vm;
    }

    public async Task UpdateProductTypeAsync(ProductTypeFormViewModel vm)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateString(vm.Name, "商品類型名稱", 50, nameof(vm.Name));

        var existing = await _unitOfWork.ProductTypes.GetByIdAsync(vm.Id);
        ValidationHelper.ValidateEntityExists(existing, "商品類型", vm.Id);

        existing!.Name = vm.Name;
        existing.Description = vm.Description;
        existing.ParentId = vm.ParentId;
        existing.DisplayOrder = vm.DisplayOrder;

        await _unitOfWork.ProductTypes.UpdateAsync(existing);
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

    public async Task<IEnumerable<SelectItemViewModel>> GetChildCategorySelectItemsAsync()
    {
        var childCategories = await _unitOfWork.ProductTypes.GetAllChildCategoriesAsync();
        return childCategories.Select(c => new SelectItemViewModel
        {
            Id = c.Id,
            Name = c.Name,
            ParentId = c.ParentId,
            ParentName = c.Parent?.Name,
            DisplayOrder = c.DisplayOrder
        });
    }
}
