using System;
using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
    // Create, Read, Update, Delete (CRUD) operations for Movie entity
    Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions moviesOptions, CancellationToken cancellationToken = default);
    Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
