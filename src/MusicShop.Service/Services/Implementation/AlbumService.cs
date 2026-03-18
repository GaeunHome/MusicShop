using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Album;
using MusicShop.Service.ViewModels.Shared;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 專輯商業邏輯實作
/// </summary>
public class AlbumService : IAlbumService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AlbumService> _logger;

    public AlbumService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AlbumService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AlbumCardViewModel>> GetAlbumCardViewModelsAsync(
        string? searchTerm = null,
        int? artistCategoryId = null,
        int? artistId = null,
        int? productTypeId = null,
        int? parentProductTypeId = null,
        string? sortBy = null,
        int? excludeId = null)
    {
        var albums = await _unitOfWork.Albums.GetAlbumsAsync(searchTerm, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy, excludeId);
        return _mapper.Map<IEnumerable<AlbumCardViewModel>>(albums);
    }

    public async Task<PagedResult<AlbumCardViewModel>> GetAlbumCardViewModelsPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        int? artistCategoryId = null,
        int? artistId = null,
        int? productTypeId = null,
        int? parentProductTypeId = null,
        string? sortBy = null)
    {
        ValidationHelper.ValidatePositive(page, "頁碼", nameof(page));
        ValidationHelper.ValidatePositive(pageSize, "每頁筆數", nameof(pageSize));

        var (albums, totalCount) = await _unitOfWork.Albums.GetAlbumsPagedAsync(
            page, pageSize, searchTerm, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy);

        var viewModels = _mapper.Map<IEnumerable<AlbumCardViewModel>>(albums);
        return new PagedResult<AlbumCardViewModel>(viewModels, totalCount, page, pageSize);
    }

    public async Task<AlbumDetailViewModel?> GetAlbumDetailViewModelAsync(int id)
    {
        var album = await _unitOfWork.Albums.GetAlbumByIdAsync(id);
        if (album == null) return null;

        // 相關商品查詢策略：使用「藝人分類 + 藝人 + 商品類型」做聯合篩選，
        // 而非僅以完全相同的藝人做精確匹配。這樣做的好處是：
        // 當同一藝人的商品不足時，仍能透過同分類（如同為 GIRL GROUP）
        // 或同商品類型（如同為 ALBUM）推薦相關商品，提升推薦的覆蓋率。
        var relatedAlbums = await _unitOfWork.Albums.GetAlbumsAsync(
            null,
            album.Artist?.ArtistCategoryId,
            album.ArtistId,
            album.ProductTypeId,
            null,
            null,
            excludeId: id);

        var vm = _mapper.Map<AlbumDetailViewModel>(album);

        // RelatedAlbums 在 MapperProfile 中設為 Ignore（因為需要額外查詢，無法在單次映射中完成），
        // 因此必須手動賦值。
        vm.RelatedAlbums = _mapper.Map<List<AlbumCardViewModel>>(
            relatedAlbums.Take(DisplayConstants.RelatedAlbumsCount).ToList());

        return vm;
    }

    public async Task<IEnumerable<AlbumCardViewModel>> GetLatestAlbumCardsAsync(int count)
    {
        ValidationHelper.ValidatePositive(count, "數量", nameof(count));

        var albums = await _unitOfWork.Albums.GetLatestAlbumsAsync(count);
        return _mapper.Map<IEnumerable<AlbumCardViewModel>>(albums);
    }

    public async Task<bool> IsStockAvailableAsync(int albumId, int quantity = 1)
    {
        ValidationHelper.ValidatePositive(quantity, "數量", nameof(quantity));

        var album = await _unitOfWork.Albums.GetAlbumByIdAsync(albumId);

        if (album == null)
        {
            return false;
        }

        return album.Stock >= quantity;
    }

    public async Task<IEnumerable<SelectItemViewModel>> GetAlbumSelectItemsAsync()
    {
        var albums = await _unitOfWork.Albums.GetAllAlbumsAsync();
        return _mapper.Map<IEnumerable<SelectItemViewModel>>(albums);
    }

    public async Task<IEnumerable<AlbumListItemViewModel>> GetAlbumListItemsAsync()
    {
        var albums = await _unitOfWork.Albums.GetAllAlbumsAsync();
        return _mapper.Map<IEnumerable<AlbumListItemViewModel>>(albums);
    }

    public async Task<AlbumFormViewModel?> GetAlbumFormByIdAsync(int id)
    {
        var album = await _unitOfWork.Albums.GetAlbumByIdAsync(id);
        if (album == null) return null;

        return _mapper.Map<AlbumFormViewModel>(album);
    }

    public async Task<AlbumFormViewModel> CreateAlbumAsync(AlbumFormViewModel vm)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateNotEmpty(vm.Title, "專輯標題", nameof(vm.Title));
        ValidationHelper.ValidatePositive(vm.Price, "價格", nameof(vm.Price));

        var newAlbum = _mapper.Map<Album>(vm);
        newAlbum.CreatedAt = DateTime.UtcNow;

        var savedAlbum = await _unitOfWork.Albums.AddAlbumAsync(newAlbum);
        await _unitOfWork.SaveChangesAsync();

        // 回寫資料庫產生的欄位到 ViewModel
        vm.Id = savedAlbum.Id;
        vm.CreatedAt = savedAlbum.CreatedAt;
        vm.RowVersion = savedAlbum.RowVersion;

        _logger.LogInformation("專輯新增成功：AlbumId={AlbumId}, Title={Title}", savedAlbum.Id, vm.Title);
        return vm;
    }

    public async Task UpdateAlbumAsync(AlbumFormViewModel vm)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateNotEmpty(vm.Title, "專輯標題", nameof(vm.Title));
        ValidationHelper.ValidatePositive(vm.Price, "價格", nameof(vm.Price));

        var albumExists = await _unitOfWork.Albums.AlbumExistsAsync(vm.Id);
        ValidationHelper.ValidateCondition(albumExists, $"找不到 ID 為 {vm.Id} 的專輯");

        // 取得現有實體並逐欄更新（需保留 RowVersion 做樂觀並發控制）
        var existingAlbum = await _unitOfWork.Albums.GetAlbumByIdAsync(vm.Id);
        existingAlbum!.Title = vm.Title;
        existingAlbum.Description = vm.Description;
        existingAlbum.Price = vm.Price;
        existingAlbum.Stock = vm.Stock;
        existingAlbum.ArtistId = vm.ArtistId;
        existingAlbum.ProductTypeId = vm.ProductTypeId;
        existingAlbum.CoverImageUrl = vm.CoverImageUrl;
        existingAlbum.DescriptionImageUrl = vm.DescriptionImageUrl;
        existingAlbum.RowVersion = vm.RowVersion;

        try
        {
            await _unitOfWork.Albums.UpdateAlbumAsync(existingAlbum);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("專輯更新成功：AlbumId={AlbumId}, Title={Title}", vm.Id, vm.Title);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "專輯並發衝突：AlbumId={AlbumId}, Title={Title}", vm.Id, vm.Title);
            throw new InvalidOperationException(
                "此商品已被其他人修改，請重新整理頁面後再試。", ex);
        }
    }

    public async Task DeleteAlbumAsync(int id)
    {
        var exists = await _unitOfWork.Albums.AlbumExistsAsync(id);
        ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {id} 的專輯");

        await _unitOfWork.Albums.DeleteAlbumAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("專輯刪除成功：AlbumId={AlbumId}", id);
    }
}
