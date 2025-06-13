using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Cache;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly IOutputCacheStore _outputCacheStore;
    public RatingsController(IRatingService ratingService, IOutputCacheStore outputCacheStore)
    {
        _ratingService = ratingService;
        _outputCacheStore = outputCacheStore;
    }

    [Authorize]
    [HttpPut(ApiEndpoints.Movies.Rate)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RateMovieAsync([FromRoute] Guid id, [FromBody] RatingRequest request, CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.RateMovieAsync(id, request.Rating, userId!.Value, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        await _outputCacheStore.EvictByTagAsync(CacheConstants.MovieCacheTag, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRatingAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.DeleteRatingAsync(id, userId!.Value, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        await _outputCacheStore.EvictByTagAsync(CacheConstants.MovieCacheTag, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
    [ProducesResponseType(typeof(IEnumerable<MovieRatingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRatingsForUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();
        var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, cancellationToken);

        var ratingsResponse = ratings.MapToResponse();
        return Ok(ratingsResponse);
    }
}
