using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers.V1
{

    [ApiController]
    [ApiVersion("1.0")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [Authorize(AuthContstants.TrusterMemberPolicyName)]
        [HttpPost(ApiEndpoints.Movies.Create)]
        public async Task<IActionResult> Create([FromBody] CreateMovieRequest movieRequest, CancellationToken cancellationToken)
        {
            var movie = movieRequest.MapToMovie();
            await _movieService.CreateAsync(movie, cancellationToken);

            return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie.MapToResponse());
        }

        /// <summary>
        /// Retrieves a paginated list of movies based on query parameters and the current user's context.
        /// </summary>
        /// <param name="moviesRequest">Query parameters for filtering, sorting, and pagination.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>A 200 OK response containing the paginated list of movies.</returns>
        [HttpGet(ApiEndpoints.Movies.GetAll)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest moviesRequest, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var options = moviesRequest.MapToOptions().WithUserId(userId);
            
            var movies = await _movieService.GetAllAsync(options, cancellationToken);
            var totalCount = await _movieService.GetCountAsync(options, cancellationToken);
            
            var response = movies.MapToResponse(options.Page, options.PageSize, totalCount);
            
            return Ok(response);
        }


        /// <summary>
                /// Retrieves a movie by its unique identifier or slug.
                /// </summary>
                /// <param name="idOrSlug">The movie's GUID or slug.</param>
                /// <param name="cancellationToken">Token to cancel the operation.</param>
                /// <returns>
                /// 200 OK with the movie details if found; otherwise, 404 Not Found.
                /// </returns>
        [HttpGet(ApiEndpoints.Movies.Get)]
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
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest movieRequest, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var movie = movieRequest.MapToMovie(id);
            var updatedMovie = await _movieService.UpdateAsync(movie, userId, cancellationToken);
            if (updatedMovie is null)
            {
                return NotFound();
            }
            return Ok(updatedMovie.MapToResponse());
        }

        [Authorize(AuthContstants.AdminUserPolicyName)]
        [HttpDelete(ApiEndpoints.Movies.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _movieService.DeleteByIdAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
