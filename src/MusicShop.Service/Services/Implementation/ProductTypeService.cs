using AutoMapper;
using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Constants;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Shared;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 商品類型商業邏輯實作
/// </summary>
public class ProductTypeService : IProductTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public ProductTypeService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
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
        var productType = await _unitOfWork.ProductTypes.GetByIdAsync(id);
        if (productType == null) return null;

        return _mapper.Map<ProductTypeFormViewModel>(productType);
    }

    public async Task<ProductTypeFormViewModel> CreateProductTypeAsync(ProductTypeFormViewModel vm)
    {
        ValidationHelper.ValidateString(vm.Name, "商品類型名稱", 50, nameof(vm.Name));

        var newType = _mapper.Map<ProductType>(vm);

        var savedType = await _unitOfWork.ProductTypes.CreateAsync(newType);
        await _unitOfWork.SaveChangesAsync();
        vm.Id = savedType.Id;

        _cacheService.RemoveByPrefix(CacheKeys.CategoriesPrefix);
        return vm;
    }

    public async Task UpdateProductTypeAsync(ProductTypeFormViewModel vm)
    {
        ValidationHelper.ValidateString(vm.Name, "商品類型名稱", 50, nameof(vm.Name));

        var existingType = await _unitOfWork.ProductTypes.GetByIdAsync(vm.Id);
        ValidationHelper.ValidateEntityExists(existingType, "商品類型", vm.Id);

        existingType!.Name = vm.Name;
        existingType.Description = vm.Description;
        existingType.ParentId = vm.ParentId;
        existingType.DisplayOrder = vm.DisplayOrder;

        await _unitOfWork.ProductTypes.UpdateAsync(existingType);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveByPrefix(CacheKeys.CategoriesPrefix);
    }

    public async Task DeleteProductTypeAsync(int id)
    {
        var targetType = await _unitOfWork.ProductTypes.GetByIdWithChildrenAsync(id);
        ValidationHelper.ValidateEntityExists(targetType, "商品類型", id);

        // 檢查是否有子分類（有的話不能刪除）
        var hasChildTypes = await _unitOfWork.ProductTypes.HasChildrenAsync(id);
        ValidationHelper.ValidateCondition(
            !hasChildTypes,
            $"無法刪除「{targetType!.Name}」，因為還有子分類"
        );

        // 檢查是否有商品使用此類型
        var linkedAlbumCount = await _unitOfWork.Albums.CountByProductTypeAsync(id);
        ValidationHelper.ValidateCondition(
            linkedAlbumCount == 0,
            $"無法刪除「{targetType.Name}」，因為還有 {linkedAlbumCount} 個商品使用此類型"
        );

        await _unitOfWork.ProductTypes.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveByPrefix(CacheKeys.CategoriesPrefix);
    }

    public async Task<IEnumerable<ProductType>> GetParentCategoriesAsync()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.ProductTypeParents,
            () => _unitOfWork.ProductTypes.GetParentCategoriesAsync());
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
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.ProductTypeChildren,
            () => _unitOfWork.ProductTypes.GetAllChildCategoriesAsync());
    }

    public async Task<IEnumerable<SelectItemViewModel>> GetParentCategorySelectItemsAsync()
    {
        var parents = await _unitOfWork.ProductTypes.GetParentCategoriesAsync();
        return _mapper.Map<IEnumerable<SelectItemViewModel>>(parents);
    }

    public async Task<IEnumerable<SelectItemViewModel>> GetChildCategorySelectItemsByParentIdAsync(int parentId)
    {
        var children = await _unitOfWork.ProductTypes.GetChildrenByParentIdAsync(parentId);
        return _mapper.Map<IEnumerable<SelectItemViewModel>>(children);
    }

    public async Task<string?> GetProductTypeNameByIdAsync(int id)
    {
        var productType = await _unitOfWork.ProductTypes.GetByIdAsync(id);
        return productType?.Name;
    }

    public async Task<int?> GetParentIdByProductTypeIdAsync(int productTypeId)
    {
        var productType = await _unitOfWork.ProductTypes.GetByIdWithChildrenAsync(productTypeId);
        return productType?.ParentId;
    }

    public async Task<IEnumerable<SelectItemViewModel>> GetChildCategorySelectItemsAsync()
    {
        var childCategories = await _unitOfWork.ProductTypes.GetAllChildCategoriesAsync();
        return _mapper.Map<IEnumerable<SelectItemViewModel>>(childCategories);
    }

    public async Task<List<ProductTypeCategoryTreeViewModel>> GetCategoryTreeViewModelsAsync()
    {
        var parents = await _unitOfWork.ProductTypes.GetParentCategoriesWithChildrenAsync();

        return parents.Select(parent => new ProductTypeCategoryTreeViewModel
        {
            Id = parent.Id,
            Name = parent.Name,
            Description = parent.Description,
            DisplayOrder = parent.DisplayOrder,
            Children = _mapper.Map<List<ProductTypeChildItemViewModel>>(parent.Children)
        }).ToList();
    }

    public async Task<List<NavCategoryItemViewModel>> GetNavCategoryTreeAsync()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.ProductTypeNavTree,
            async () =>
            {
                var parents = await _unitOfWork.ProductTypes.GetParentCategoriesWithChildrenAsync();

                return parents.Select(parent => new NavCategoryItemViewModel
                {
                    Id = parent.Id,
                    Name = parent.Name,
                    DisplayOrder = parent.DisplayOrder,
                    Children = parent.Children.Select(c => new NavCategoryItemViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        DisplayOrder = c.DisplayOrder
                    }).ToList()
                }).ToList();
            });
    }
}
