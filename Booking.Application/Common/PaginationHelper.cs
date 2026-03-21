namespace Booking.Application.Common;

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<T> Data { get; set; } = new();
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    public PagedResult(List<T> data, int count, int page, int pageSize)
    {
        Data = data;
        TotalCount = count;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        HasNextPage = Page < TotalPages;
        HasPreviousPage = Page > 1;
    }
}