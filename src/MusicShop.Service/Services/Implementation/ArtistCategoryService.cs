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
/// 藝人分類商業邏輯實作
/// </summary>
public class ArtistCategoryService : IArtistCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public ArtistCategoryService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<ArtistCategory>> GetAllArtistCategoriesAsync()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.ArtistCategories,
            () => _unitOfWork.ArtistCategories.GetAllAsync());
    }

    public async Task<IEnumerable<SelectItemViewModel>> GetArtistCategorySelectItemsAsync()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.ArtistCategorySelectItems,
            async () =>
            {
                var categories = await _unitOfWork.ArtistCategories.GetAllAsync();
                return categories.Select(c => new SelectItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                });
            });
    }

    public async Task<ArtistCategoryFormViewModel?> GetArtistCategoryFormByIdAsync(int id)
    {
        var category = await _unitOfWork.ArtistCategories.GetByIdAsync(id);
        if (category == null) return null;

        return _mapper.Map<ArtistCategoryFormViewModel>(category);
    }

    public async Task<ArtistCategoryFormViewModel> CreateArtistCategoryAsync(ArtistCategoryFormViewModel vm)
    {
        ValidationHelper.ValidateString(vm.Name, "藝人分類名稱", 50, nameof(vm.Name));

        var newCategory = _mapper.Map<ArtistCategory>(vm);

        var savedCategory = await _unitOfWork.ArtistCategories.CreateAsync(newCategory);
        await _unitOfWork.SaveChangesAsync();
        vm.Id = savedCategory.Id;

        _cacheService.RemoveByPrefix(CacheKeys.CategoriesPrefix);
        return vm;
    }

    public async Task UpdateArtistCategoryAsync(ArtistCategoryFormViewModel vm)
    {
        ValidationHelper.ValidateString(vm.Name, "藝人分類名稱", 50, nameof(vm.Name));

        var existingCategory = await _unitOfWork.ArtistCategories.GetByIdAsync(vm.Id);
        ValidationHelper.ValidateEntityExists(existingCategory, "藝人分類", vm.Id);

        existingCategory!.Name = vm.Name;
        existingCategory.Description = vm.Description;
        existingCategory.DisplayOrder = vm.DisplayOrder;

        await _unitOfWork.ArtistCategories.UpdateAsync(existingCategory);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveByPrefix(CacheKeys.CategoriesPrefix);
    }

    public async Task DeleteArtistCategoryAsync(int id)
    {
        var targetCategory = await _unitOfWork.ArtistCategories.GetByIdAsync(id);
        ValidationHelper.ValidateEntityExists(targetCategory, "藝人分類", id);

        // 檢查是否有商品使用此分類（有的話不能刪除）
        var linkedAlbumCount = await _unitOfWork.Albums.CountByArtistCategoryAsync(id);
        ValidationHelper.ValidateCondition(
            linkedAlbumCount == 0,
            $"無法刪除「{targetCategory!.Name}」，因為還有 {linkedAlbumCount} 個商品使用此分類"
        );

        await _unitOfWork.ArtistCategories.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveByPrefix(CacheKeys.CategoriesPrefix);
    }

    public async Task<List<ArtistCategoryListItemViewModel>> GetArtistCategoryListItemsAsync()
    {
        var categories = await _unitOfWork.ArtistCategories.GetAllAsync();
        return _mapper.Map<List<ArtistCategoryListItemViewModel>>(categories);
    }
}
