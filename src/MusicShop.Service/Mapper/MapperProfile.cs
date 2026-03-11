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
            // User 映射
            CreateMap<AppUser, EditProfileViewModel>()
                .ReverseMap();

            CreateMap<AppUser, UserManagementViewModel>();
        }
    }
}
