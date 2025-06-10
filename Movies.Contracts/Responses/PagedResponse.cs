using System;

namespace Movies.Contracts.Responses;

public class PagedResponse<T>
{
    public required IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public bool HasNextPage { get; init; }
}