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
        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        // [MapToApiVersion("2.0")]
        // [HttpGet(ApiEndpoints.Movies.Get)]
        // public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken cancellationToken)
        // {
        //     var userId = HttpContext.GetUserId();
        //     var movie = Guid.TryParse(idOrSlug, out var id)
        //         ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
        //         : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

        //     if (movie is null)
        //     {
        //         return NotFound();
        //     }
        //     var response = movie.MapToResponse();
        //     return Ok(response);
        // }
    }
}
