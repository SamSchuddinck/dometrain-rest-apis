using System;

namespace Movies.Application.Models;

public class PagedResult<T>
{
    public required IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public bool HasNextPage => Page * PageSize < Total;
}