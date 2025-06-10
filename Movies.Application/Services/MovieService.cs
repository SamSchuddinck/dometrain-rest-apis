using System;
using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Contracts.Responses;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IValidator<Movie> _movieValidator;

    private readonly IValidator<GetAllMoviesOptions> _optionsValidator;

    public MovieService(IMovieRepository movieRepository, IRatingRepository ratingRepository, IValidator<Movie> movieValidator, IValidator<GetAllMoviesOptions> optionsValidator)
    {
        _movieRepository = movieRepository;
        _ratingRepository = ratingRepository;
        _movieValidator = movieValidator;
        _optionsValidator = optionsValidator;
    }

    public async Task<bool> CreateAsync(Movie movie,  CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        return await _movieRepository.CreateAsync(movie, cancellationToken);
    }


    public async Task<MoviesResponse> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default)
    {
        await _optionsValidator.ValidateAndThrowAsync(options, cancellationToken);

        var movies = await _movieRepository.GetAllAsync(options, cancellationToken);
        var totalCount = await _movieRepository.GetCountAsync(options, cancellationToken);

        return new MoviesResponse
        {
            Items = movies.Select(movie => new MovieResponse
            {
                Id = movie.Id,
                Title = movie.Title,
                Slug = movie.Slug,
                YearOfRelease = movie.YearOfRelease,
                Rating = movie.Rating,
                UserRating = movie.UserRating,
                Genres = movie.Genres
            }),
            Page = options.Page,
            PageSize = options.PageSize,
            Total = totalCount,
            HasNextPage = options.Page * options.PageSize < totalCount
        };
    }

    public Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        return _movieRepository.GetByIdAsync(id, userId, cancellationToken);
    }

    public Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        return _movieRepository.GetBySlugAsync(slug, userId, cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default,CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, cancellationToken);
        if (!movieExists)
        {
            return null;
        }

        await _movieRepository.UpdateAsync(movie, cancellationToken);

        if (!userId.HasValue)
        {
            var rating = await _ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
            movie.Rating = rating;
            return movie;
        }

        var ratings = await _ratingRepository.GetRatingAsync(movie.Id, userId.Value, cancellationToken);
        movie.Rating = ratings.Rating;
        movie.UserRating = ratings.UserRating;

        return movie;
    }
    
    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _movieRepository.DeleteByIdAsync(id, cancellationToken);
    }
}
