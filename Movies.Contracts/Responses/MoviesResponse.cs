using System;

namespace Movies.Contracts.Responses;

public class MoviesResponse : PagedResponse<MovieResponse>
{
    // Inherits all pagination properties from PagedResponse<MovieResponse>
    // Items, Page, PageSize, Total, HasNextPage
}
