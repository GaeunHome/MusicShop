namespace MusicShop.Library.Helpers;

/// <summary>
/// 分頁查詢結果的通用封裝
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int CurrentPage { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public PagedResult(IEnumerable<T> items, int totalCount, int currentPage, int pageSize)
    {
        Items = items.ToList().AsReadOnly();
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }
}
