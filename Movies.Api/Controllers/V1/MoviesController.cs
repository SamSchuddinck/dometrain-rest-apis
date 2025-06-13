using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers.V1
{

    [ApiController]
    [ApiVersion("1.0")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IOutputCacheStore _outputCacheStore;

        public MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore)
        {
            _outputCacheStore = outputCacheStore;
            _movieService = movieService;
        }

        [Authorize(AuthContstants.TrusterMemberPolicyName)]
        [HttpPost(ApiEndpoints.Movies.Create)]
        [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateMovieRequest movieRequest, CancellationToken cancellationToken)
        {
            var movie = movieRequest.MapToMovie();
            await _movieService.CreateAsync(movie, cancellationToken);
            await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);   
            return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie.MapToResponse());
        }

        [HttpGet(ApiEndpoints.Movies.GetAll)]
        [ProducesResponseType(typeof(PagedResponse<MovieResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        [OutputCache(PolicyName = "MoviesCache")]
        // [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "title", "year", "sortBy", "page", "pageSize" }, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest moviesRequest, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var options = moviesRequest.MapToOptions().WithUserId(userId);

            var movies = await _movieService.GetAllAsync(options, cancellationToken);
            var totalCount = await _movieService.GetCountAsync(options, cancellationToken);

            var response = movies.MapToResponse(options.Page, options.PageSize, totalCount);

            return Ok(response);
        }


        [HttpGet(ApiEndpoints.Movies.Get)]
        [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OutputCache(PolicyName = "MovieCache")]
        // [ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var movie = Guid.TryParse(idOrSlug, out var id)
                ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
                : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

            if (movie is null)
            {
                return NotFound();
            }
            var response = movie.MapToResponse();
            return Ok(response);
        }

        [Authorize(AuthContstants.TrusterMemberPolicyName)]
        [HttpPut(ApiEndpoints.Movies.Update)]
        [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest movieRequest, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var movie = movieRequest.MapToMovie(id);
            var updatedMovie = await _movieService.UpdateAsync(movie, userId, cancellationToken);
            if (updatedMovie is null)
            {
                return NotFound();
            }
            await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
            return Ok(updatedMovie.MapToResponse());
        }

        [Authorize(AuthContstants.AdminUserPolicyName)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete(ApiEndpoints.Movies.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _movieService.DeleteByIdAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }
            await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
            return Ok();
        }
    }
}
