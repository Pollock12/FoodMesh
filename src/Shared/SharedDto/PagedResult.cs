namespace FoodMesh.Shared.SharedDto;

/// <summary>
/// Standard pagination wrapper for listing queries.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int pageNumber, int pageSize, long totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
