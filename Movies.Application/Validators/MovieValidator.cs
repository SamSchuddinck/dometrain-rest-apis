using System;
using System.Data;
using FluentValidation;
using Movies.Application.Repositories;

namespace Movies.Application.Validators;

public class MovieValidator: AbstractValidator<Movie>
{
    private readonly IMovieRepository _movieRepository;

    public MovieValidator(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
        
        RuleFor(movie => movie.Id).NotEmpty();

        RuleFor(movie => movie.Genres).NotEmpty();

        RuleFor(movie => movie.Title).NotEmpty();

        RuleFor(movie => movie.YearOfRelease)
        .LessThanOrEqualTo(DateTime.Now.Year);

        RuleFor(movie => movie.Slug)
        .MustAsync(ValidateSlug)
        .WithMessage("This movie already exists.");

    }

    private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken cancellationToken)
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(slug);

        if (existingMovie is not null)
        {
            return existingMovie.Id == movie.Id;
        }

        return existingMovie is null;
    }
}
