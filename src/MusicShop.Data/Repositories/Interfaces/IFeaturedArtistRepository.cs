using MusicShop.Data.Entities;

namespace MusicShop.Data.Repositories.Interfaces;

public interface IFeaturedArtistRepository : IGenericRepository<FeaturedArtist>
{
    /// <summary>
    /// 取得啟用中的精選藝人（Include Artist + Albums，依 DisplayOrder 排序）
    /// </summary>
    Task<IEnumerable<FeaturedArtist>> GetActiveFeaturedArtistsAsync();

    /// <summary>
    /// 取得所有精選藝人（Admin 列表用，依 DisplayOrder 排序）
    /// </summary>
    Task<IEnumerable<FeaturedArtist>> GetAllOrderedAsync();

}
