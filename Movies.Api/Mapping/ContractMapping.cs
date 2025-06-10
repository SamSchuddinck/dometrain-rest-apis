using System;
using Microsoft.AspNetCore.StaticAssets;
using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
    public static Movie MapToMovie(this CreateMovieRequest request)
    {
        return new Movie
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };
    }

    public static Movie MapToMovie(this UpdateMovieRequest request, Guid existingMovieId)
    {
        return new Movie
        {
            Id = existingMovieId,
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };
    }

    public static MovieResponse MapToResponse(this Movie movie)
    {
        return new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Slug = movie.Slug,
            YearOfRelease = movie.YearOfRelease,
            Rating = movie.Rating,
            UserRating = movie.UserRating,
            Genres = movie.Genres
        };
    }

    public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies)
    {
        return new MoviesResponse
        {
            Items = movies.Select(m => m.MapToResponse())
        };
    }

    public static MovieRatingResponse MapToResponse(this MovieRating movieRating)
    {
        return new MovieRatingResponse
        {
            MovieId = movieRating.MovieId,
            Slug = movieRating.Slug,
            Rating = movieRating.Rating
        };
    }

    public static IEnumerable<MovieRatingResponse> MapToResponse(this IEnumerable<MovieRating> movieRatings)
    {
        return movieRatings.Select(movieRating => new MovieRatingResponse
        {
            MovieId = movieRating.MovieId,
            Slug = movieRating.Slug,
            Rating = movieRating.Rating
        });
    }

    public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
    {
        return new GetAllMoviesOptions
        {
            Title = request.Title,
            YearOfRelease = request.Year,
            SortField = request.SortBy?.Trim('+', '-', ' '),
            SortOrder = request.SortBy is null ? SortOrder.Unsorted :
                         request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public static GetAllMoviesOptions WithUserId(this GetAllMoviesOptions options, Guid? userId)
    {
        options.UserId = userId;
        return options;
    }
}
