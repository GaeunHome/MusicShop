using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels;
using MusicShop.Service.ViewModels.Admin;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 藝人分類商業邏輯實作
/// </summary>
public class ArtistCategoryService : IArtistCategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public ArtistCategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ArtistCategory>> GetAllArtistCategoriesAsync()
    {
        return await _unitOfWork.ArtistCategories.GetAllAsync();
    }

    public async Task<IEnumerable<SelectItemViewModel>> GetArtistCategorySelectItemsAsync()
    {
        var categories = await _unitOfWork.ArtistCategories.GetAllAsync();
        return categories.Select(c => new SelectItemViewModel
        {
            Id = c.Id,
            Name = c.Name
        });
    }

    public async Task<ArtistCategoryFormViewModel?> GetArtistCategoryFormByIdAsync(int id)
    {
        var entity = await _unitOfWork.ArtistCategories.GetByIdAsync(id);
        if (entity == null) return null;

        return new ArtistCategoryFormViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            DisplayOrder = entity.DisplayOrder
        };
    }

    public async Task<ArtistCategoryFormViewModel> CreateArtistCategoryAsync(ArtistCategoryFormViewModel vm)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateString(vm.Name, "藝人分類名稱", 50, nameof(vm.Name));

        var entity = new ArtistCategory
        {
            Name = vm.Name,
            Description = vm.Description,
            DisplayOrder = vm.DisplayOrder
        };

        var created = await _unitOfWork.ArtistCategories.CreateAsync(entity);
        vm.Id = created.Id;
        return vm;
    }

    public async Task UpdateArtistCategoryAsync(ArtistCategoryFormViewModel vm)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateString(vm.Name, "藝人分類名稱", 50, nameof(vm.Name));

        var existing = await _unitOfWork.ArtistCategories.GetByIdAsync(vm.Id);
        ValidationHelper.ValidateEntityExists(existing, "藝人分類", vm.Id);

        existing!.Name = vm.Name;
        existing.Description = vm.Description;
        existing.DisplayOrder = vm.DisplayOrder;

        await _unitOfWork.ArtistCategories.UpdateAsync(existing);
    }

    public async Task DeleteArtistCategoryAsync(int id)
    {
        var exists = await _unitOfWork.ArtistCategories.GetByIdAsync(id);
        ValidationHelper.ValidateEntityExists(exists, "藝人分類", id);

        // 檢查是否有商品使用此分類
        var albums = await _unitOfWork.Albums.GetAlbumsAsync(null, id, null);
        ValidationHelper.ValidateCondition(
            !albums.Any(),
            $"無法刪除「{exists!.Name}」，因為還有 {albums.Count()} 個商品使用此分類"
        );

        await _unitOfWork.ArtistCategories.DeleteAsync(id);
    }
}
