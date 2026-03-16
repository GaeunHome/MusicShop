using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Extensions;
using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Album;

namespace MusicShop.Service.Services.Implementation
{
    /// <summary>
    /// 專輯商業邏輯實作
    /// </summary>
    public class AlbumService : IAlbumService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlbumService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Album>> GetAlbumsAsync(
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? artistId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null,
            string? sortBy = null)
        {
            return await _unitOfWork.Albums.GetAlbumsAsync(searchTerm, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy);
        }

        public async Task<IEnumerable<AlbumCardViewModel>> GetAlbumCardViewModelsAsync(
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? artistId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null,
            string? sortBy = null)
        {
            var albums = await _unitOfWork.Albums.GetAlbumsAsync(searchTerm, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy);
            return albums.Select(MapToAlbumCardViewModel);
        }

        public async Task<Album?> GetAlbumDetailAsync(int id)
        {
            return await _unitOfWork.Albums.GetAlbumByIdAsync(id);
        }

        public async Task<AlbumDetailViewModel?> GetAlbumDetailViewModelAsync(int id)
        {
            var album = await _unitOfWork.Albums.GetAlbumByIdAsync(id);
            if (album == null) return null;

            // 取得相關商品（同藝人或同商品類型，排除當前商品）
            var relatedAlbums = await _unitOfWork.Albums.GetAlbumsAsync(
                null,
                album.Artist?.ArtistCategoryId,
                album.ArtistId,
                album.ProductTypeId,
                null);

            var imageUrls = !string.IsNullOrEmpty(album.CoverImageUrl)
                ? album.CoverImageUrl.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();

            return new AlbumDetailViewModel
            {
                Id = album.Id,
                Title = album.Title,
                Description = album.Description,
                DescriptionImageUrl = album.DescriptionImageUrl,
                Price = album.Price,
                Stock = album.Stock,
                CoverImageUrl = album.CoverImageUrl,
                ArtistName = album.Artist?.Name,
                ArtistId = album.ArtistId,
                ArtistCategoryId = album.Artist?.ArtistCategoryId,
                ProductTypeId = album.ProductTypeId,
                ImageUrls = imageUrls,
                RelatedAlbums = relatedAlbums
                    .Where(a => a.Id != id)
                    .Take(8)
                    .Select(MapToAlbumCardViewModel)
                    .ToList()
            };
        }

        public async Task<IEnumerable<AlbumCardViewModel>> GetLatestAlbumCardsAsync(int count)
        {
            ValidationHelper.ValidatePositive(count, "數量", nameof(count));

            var albums = await _unitOfWork.Albums.GetLatestAlbumsAsync(count);
            return albums.Select(MapToAlbumCardViewModel);
        }

        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int count)
        {
            ValidationHelper.ValidatePositive(count, "數量", nameof(count));

            return await _unitOfWork.Albums.GetLatestAlbumsAsync(count);
        }

        /// <summary>
        /// 將 Album Entity 映射為 AlbumCardViewModel
        /// </summary>
        private static AlbumCardViewModel MapToAlbumCardViewModel(Album album)
        {
            // 取第一張圖片 URL
            string? firstCoverUrl = null;
            if (!string.IsNullOrEmpty(album.CoverImageUrl))
            {
                var urls = album.CoverImageUrl.Split(',', StringSplitOptions.RemoveEmptyEntries);
                firstCoverUrl = urls.Length > 0 ? urls[0] : null;
            }

            return new AlbumCardViewModel
            {
                Id = album.Id,
                Title = album.Title,
                Price = album.Price,
                Stock = album.Stock,
                CoverImageUrl = firstCoverUrl,
                ArtistName = album.Artist?.Name,
                ArtistId = album.ArtistId,
                ArtistCategoryName = album.Artist?.ArtistCategory?.Name,
                ProductTypeName = album.ProductType?.Name
            };
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

        public async Task<IEnumerable<AlbumListItemViewModel>> GetAlbumListItemsAsync()
        {
            var albums = await _unitOfWork.Albums.GetAllAlbumsAsync();
            return albums.Select(a => new AlbumListItemViewModel
            {
                Id = a.Id,
                Title = a.Title,
                ArtistName = a.Artist?.Name,
                ArtistCategoryName = a.Artist?.ArtistCategory?.Name,
                ProductTypeName = a.ProductType?.Name,
                Price = a.Price,
                Stock = a.Stock
            });
        }

        public async Task<AlbumFormViewModel?> GetAlbumFormByIdAsync(int id)
        {
            var album = await _unitOfWork.Albums.GetAlbumByIdAsync(id);
            if (album == null) return null;

            return new AlbumFormViewModel
            {
                Id = album.Id,
                Title = album.Title,
                Description = album.Description,
                Price = album.Price,
                Stock = album.Stock,
                ArtistId = album.ArtistId,
                ProductTypeId = album.ProductTypeId,
                CoverImageUrl = album.CoverImageUrl,
                DescriptionImageUrl = album.DescriptionImageUrl,
                RowVersion = album.RowVersion,
                CreatedAt = album.CreatedAt
            };
        }

        public async Task<AlbumFormViewModel> CreateAlbumAsync(AlbumFormViewModel vm)
        {
            // 商業邏輯驗證
            ValidationHelper.ValidateNotEmpty(vm.Title, "專輯標題", nameof(vm.Title));
            ValidationHelper.ValidatePositive(vm.Price, "價格", nameof(vm.Price));

            var album = new Album
            {
                Title = vm.Title,
                Description = vm.Description,
                Price = vm.Price,
                Stock = vm.Stock,
                ArtistId = vm.ArtistId,
                ProductTypeId = vm.ProductTypeId,
                CoverImageUrl = vm.CoverImageUrl,
                DescriptionImageUrl = vm.DescriptionImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _unitOfWork.Albums.AddAlbumAsync(album);
            vm.Id = created.Id;
            vm.CreatedAt = created.CreatedAt;
            vm.RowVersion = created.RowVersion;
            return vm;
        }

        public async Task UpdateAlbumAsync(AlbumFormViewModel vm)
        {
            // 商業邏輯驗證
            ValidationHelper.ValidateNotEmpty(vm.Title, "專輯標題", nameof(vm.Title));
            ValidationHelper.ValidatePositive(vm.Price, "價格", nameof(vm.Price));

            var exists = await _unitOfWork.Albums.AlbumExistsAsync(vm.Id);
            ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {vm.Id} 的專輯");

            // 取得現有實體並更新欄位
            var album = await _unitOfWork.Albums.GetAlbumByIdAsync(vm.Id);
            album!.Title = vm.Title;
            album.Description = vm.Description;
            album.Price = vm.Price;
            album.Stock = vm.Stock;
            album.ArtistId = vm.ArtistId;
            album.ProductTypeId = vm.ProductTypeId;
            album.CoverImageUrl = vm.CoverImageUrl;
            album.DescriptionImageUrl = vm.DescriptionImageUrl;
            album.RowVersion = vm.RowVersion;

            await _unitOfWork.Albums.UpdateAlbumAsync(album);
        }

        public async Task DeleteAlbumAsync(int id)
        {
            var exists = await _unitOfWork.Albums.AlbumExistsAsync(id);
            ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {id} 的專輯");

            await _unitOfWork.Albums.DeleteAlbumAsync(id);
        }
    }
}
