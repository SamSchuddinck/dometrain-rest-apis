using System;

namespace Movies.Api.Cache;

public static class CacheConstants
{
    public const string MovieCachePolicy = "MovieCache";
    public const string MovieCacheTag = "movies";
    
    public static readonly string[] MovieCacheVaryByQueryParams = new[]
    {
        "title",
        "year",
        "sortBy",
        "page",
        "pageSize"
    };
}
