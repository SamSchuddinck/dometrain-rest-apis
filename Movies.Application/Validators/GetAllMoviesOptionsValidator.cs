using System;
using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator: AbstractValidator<GetAllMoviesOptions>
{

    public static readonly string[] AcceptedSortFields = {"title", "year_of_release"};

    public GetAllMoviesOptionsValidator()
    {
        RuleFor(options => options.YearOfRelease)
            .LessThanOrEqualTo(DateTime.Now.Year);

        RuleFor(options => options.SortField)
            .Must(field => field is null || AcceptedSortFields.Contains(field, StringComparer.OrdinalIgnoreCase))
            .WithMessage("You can inly sort by 'title' or 'year_of_release'.");


    }
}
