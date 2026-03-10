using AutoMapper;
using MusicShop.Data.Entities;
using MusicShop.Service.ViewModels.Account;
using MusicShop.Service.ViewModels.Album;
using MusicShop.Service.ViewModels.Admin;

namespace MusicShop.Service.Mapper
{
    /// <summary>
    /// AutoMapper 映射設定檔
    /// 定義 Entity 和 ViewModel 之間的映射規則
    /// </summary>
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // Album 映射（暫時註解，AlbumViewModel 不存在）
            // CreateMap<Album, AlbumViewModel>()
            //     .ForMember(dest => dest.ArtistName,
            //                opt => opt.MapFrom(src => src.Artist != null ? src.Artist.Name : string.Empty))
            //     .ForMember(dest => dest.ArtistCategoryName,
            //                opt => opt.MapFrom(src => src.Artist != null && src.Artist.ArtistCategory != null
            //                                          ? src.Artist.ArtistCategory.Name
            //                                          : string.Empty))
            //     .ForMember(dest => dest.ProductTypeName,
            //                opt => opt.MapFrom(src => src.ProductType != null ? src.ProductType.Name : string.Empty));

            // User 映射
            CreateMap<AppUser, EditProfileViewModel>()
                .ReverseMap();

            CreateMap<AppUser, UserManagementViewModel>();

            // Order 映射（如需要可擴充）
            // CreateMap<Order, OrderViewModel>();
        }
    }
}
