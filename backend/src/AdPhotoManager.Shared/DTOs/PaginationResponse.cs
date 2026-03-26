namespace AdPhotoManager.Shared.DTOs;

public class PaginationResponse<T>
{
    public List<T> Data { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = new();

    public PaginationResponse()
    {
    }

    public PaginationResponse(List<T> data, int page, int pageSize, int totalItems)
    {
        Data = data;
        Pagination = new PaginationMetadata
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            HasNextPage = page < (int)Math.Ceiling(totalItems / (double)pageSize),
            HasPreviousPage = page > 1
        };
    }
}

public class PaginationMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
