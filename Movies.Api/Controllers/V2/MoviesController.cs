using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers.V2
{

    [ApiController]
    [ApiVersion("2.0")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        /// <summary>
        /// Initializes a new instance of the <see cref="MoviesController"/> with the specified movie service.
        /// </summary>
        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        /// <summary>
        /// Retrieves movie details by either a GUID identifier or a slug.
        /// </summary>
        /// <param name="idOrSlug">The movie's GUID or slug provided as a route parameter.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// Returns a 200 OK response with movie details if found; otherwise, returns 404 Not Found.
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
    }
}
