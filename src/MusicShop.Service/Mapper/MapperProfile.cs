using AutoMapper;
using MusicShop.Data.Entities;
using MusicShop.Library.Enums;
using MusicShop.Service.ViewModels.Account;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Album;
using MusicShop.Service.ViewModels.Cart;
using MusicShop.Service.ViewModels.Coupon;
using MusicShop.Service.ViewModels.Home;
using MusicShop.Service.ViewModels.Shared;
using MusicShop.Service.ViewModels.Order;
using MusicShop.Service.ViewModels.Wishlist;
using MusicShop.Library.Helpers;

namespace MusicShop.Service.Mapper
{
    /// <summary>
    /// AutoMapper 映射設定檔
    /// 集中管理所有 Entity ↔ ViewModel 的映射規則
    /// 改名時只需修改 Entity + ViewModel，映射自動對應
    /// </summary>
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // ==================== User 映射 ====================
            CreateMap<AppUser, EditProfileViewModel>()
                .ReverseMap();

            CreateMap<AppUser, UserManagementViewModel>();

            // ==================== Album 映射 ====================
            // 導航屬性（Artist、ArtistCategory）可能為 null（未 Include 或資料不存在），
            // 因此所有透過導航屬性取值的映射都需加上 null 檢查，避免 NullReferenceException。
            CreateMap<Album, AlbumCardViewModel>()
                .ForMember(d => d.ArtistCategoryName,
                    o => o.MapFrom(s => s.Artist != null && s.Artist.ArtistCategory != null
                        ? s.Artist.ArtistCategory.Name : null))
                // 卡片只需顯示第一張封面圖，透過 GetFirstCoverUrl 從逗號分隔的多圖 URL 中取出首張
                .ForMember(d => d.CoverImageUrl,
                    o => o.MapFrom(s => GetFirstCoverUrl(s.CoverImageUrl)));

            CreateMap<Album, AlbumDetailViewModel>()
                .ForMember(d => d.ArtistCategoryId,
                    o => o.MapFrom(s => s.Artist != null ? s.Artist.ArtistCategoryId : (int?)null))
                // 詳細頁需要所有圖片，將逗號分隔字串拆分為 List 供輪播元件使用
                .ForMember(d => d.ImageUrls,
                    o => o.MapFrom(s => SplitCoverUrls(s.CoverImageUrl)))
                // RelatedAlbums 需要額外的資料庫查詢（依分類篩選），無法在映射階段完成，
                // 由 AlbumService.GetAlbumDetailViewModelAsync 手動賦值
                .ForMember(d => d.RelatedAlbums, o => o.Ignore());

            CreateMap<Album, AlbumListItemViewModel>()
                .ForMember(d => d.ArtistCategoryName,
                    o => o.MapFrom(s => s.Artist != null && s.Artist.ArtistCategory != null
                        ? s.Artist.ArtistCategory.Name : null));

            CreateMap<Album, AlbumFormViewModel>()
                .ReverseMap()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.RowVersion, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore());

            // ==================== Artist 映射 ====================
            CreateMap<Artist, ArtistFormViewModel>()
                .ReverseMap();

            CreateMap<Artist, ArtistListItemViewModel>()
                .ForMember(d => d.AlbumCount,
                    o => o.MapFrom(s => s.Albums != null ? s.Albums.Count : 0));

            CreateMap<Artist, NavArtistItemViewModel>();

            // ==================== ArtistCategory 映射 ====================
            CreateMap<ArtistCategory, ArtistCategoryFormViewModel>()
                .ReverseMap();

            CreateMap<ArtistCategory, ArtistCategoryListItemViewModel>();

            // ==================== ProductType 映射 ====================
            CreateMap<ProductType, ProductTypeFormViewModel>()
                .ReverseMap();

            CreateMap<ProductType, ProductTypeChildItemViewModel>();

            CreateMap<ProductType, NavCategoryItemViewModel>()
                .ForMember(d => d.Children, o => o.Ignore());

            // ==================== FeaturedArtist 映射 ====================
            CreateMap<FeaturedArtist, FeaturedArtistListItemViewModel>()
                .ForMember(d => d.ArtistName, o => o.MapFrom(s => s.Artist.Name))
                .ForMember(d => d.ProfileImageUrl, o => o.MapFrom(s => s.Artist.ProfileImageUrl))
                .ForMember(d => d.AlbumCount, o => o.MapFrom(s => s.Artist.Albums != null ? s.Artist.Albums.Count : 0));

            CreateMap<FeaturedArtist, FeaturedArtistFormViewModel>()
                .ReverseMap()
                .ForMember(d => d.Id, o => o.Ignore());

            // ==================== Banner 映射 ====================
            CreateMap<Banner, BannerDisplayViewModel>();

            CreateMap<Banner, BannerListItemViewModel>();

            CreateMap<Banner, BannerFormViewModel>()
                .ReverseMap()
                .ForMember(d => d.Id, o => o.Ignore());

            // ==================== Cart 映射 ====================
            CreateMap<CartItem, CartItemViewModel>()
                .ForMember(d => d.AlbumTitle,
                    o => o.MapFrom(s => s.Album != null ? s.Album.Title : DisplayConstants.UnknownProduct))
                .ForMember(d => d.CoverImageUrl,
                    o => o.MapFrom(s => s.Album != null ? s.Album.CoverImageUrl : null))
                .ForMember(d => d.Price,
                    o => o.MapFrom(s => s.Album != null ? s.Album.Price : 0m))
                .ForMember(d => d.MaxStock,
                    o => o.MapFrom(s => s.Album != null ? s.Album.Stock : 0));

            // ==================== Wishlist 映射 ====================
            CreateMap<WishlistItem, WishlistItemViewModel>()
                .ForMember(d => d.AlbumTitle,
                    o => o.MapFrom(s => s.Album != null ? s.Album.Title : DisplayConstants.UnknownProduct))
                .ForMember(d => d.ArtistName,
                    o => o.MapFrom(s => s.Album != null && s.Album.Artist != null
                        ? s.Album.Artist.Name : null))
                .ForMember(d => d.CoverImageUrl,
                    o => o.MapFrom(s => s.Album != null ? s.Album.CoverImageUrl : null))
                .ForMember(d => d.Price,
                    o => o.MapFrom(s => s.Album != null ? s.Album.Price : 0m))
                .ForMember(d => d.Stock,
                    o => o.MapFrom(s => s.Album != null ? s.Album.Stock : 0));

            // ==================== Coupon 映射 ====================
            CreateMap<Coupon, CouponListItemViewModel>()
                .ForMember(d => d.IssuedCount, o => o.MapFrom(s => s.UserCoupons.Count))
                .ForMember(d => d.UsedCount, o => o.MapFrom(s => s.UserCoupons.Count(uc => uc.IsUsed)));

            CreateMap<Coupon, CouponFormViewModel>();

            CreateMap<UserCoupon, UserCouponViewModel>()
                .ForMember(d => d.CouponName, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.Name : DisplayConstants.Unknown))
                .ForMember(d => d.CouponDescription, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.Description : null))
                .ForMember(d => d.CouponCode, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.Code : ""))
                .ForMember(d => d.DiscountType, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.DiscountType : DiscountType.FixedAmount))
                .ForMember(d => d.DiscountValue, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.DiscountValue : 0m))
                .ForMember(d => d.MaxDiscountAmount, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.MaxDiscountAmount : null));

            CreateMap<UserCoupon, AvailableCouponViewModel>()
                .ForMember(d => d.UserCouponId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.CouponName, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.Name : DisplayConstants.Unknown))
                .ForMember(d => d.DiscountType, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.DiscountType : DiscountType.FixedAmount))
                .ForMember(d => d.DiscountValue, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.DiscountValue : 0m))
                .ForMember(d => d.MaxDiscountAmount, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.MaxDiscountAmount : null));

            // ==================== SystemSetting 映射 ====================
            CreateMap<SystemSetting, SystemSettingListItemViewModel>();

            CreateMap<SystemSetting, SystemSettingFormViewModel>()
                .ReverseMap()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore());

            // ==================== SelectItemViewModel 通用映射 ====================
            // 用於各種下拉選單：藝人、藝人分類、商品類型等（Name → Name 直接對應）
            CreateMap<Artist, SelectItemViewModel>();
            CreateMap<ArtistCategory, SelectItemViewModel>();
            CreateMap<ProductType, SelectItemViewModel>()
                .ForMember(d => d.ParentName, o => o.MapFrom(s => s.Parent != null ? s.Parent.Name : null));
            CreateMap<Album, SelectItemViewModel>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Title));

            // ==================== Order 子項目映射 ====================
            CreateMap<OrderItem, OrderItemViewModel>()
                .ForMember(d => d.AlbumTitle,
                    o => o.MapFrom(s => s.Album != null ? s.Album.Title : DisplayConstants.UnknownProduct))
                .ForMember(d => d.CoverImageUrl,
                    o => o.MapFrom(s => s.Album != null ? s.Album.CoverImageUrl : null));

            CreateMap<OrderItem, OrderItemSummaryViewModel>()
                .ForMember(d => d.AlbumTitle,
                    o => o.MapFrom(s => s.Album != null ? s.Album.Title : DisplayConstants.UnknownProduct));

            CreateMap<OrderItem, OrderConfirmationItemViewModel>()
                .ForMember(d => d.AlbumTitle,
                    o => o.MapFrom(s => s.Album != null ? s.Album.Title : DisplayConstants.UnknownProduct));
        }

        /// <summary>
        /// 從逗號分隔的圖片 URL 字串取得第一張圖片
        /// </summary>
        private static string? GetFirstCoverUrl(string? coverImageUrl)
        {
            if (string.IsNullOrEmpty(coverImageUrl)) return null;
            var urls = coverImageUrl.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return urls.Length > 0 ? urls[0] : null;
        }

        /// <summary>
        /// 將逗號分隔的圖片 URL 字串拆分為列表
        /// </summary>
        private static List<string> SplitCoverUrls(string? coverImageUrl)
        {
            if (string.IsNullOrEmpty(coverImageUrl)) return new List<string>();
            return coverImageUrl.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
