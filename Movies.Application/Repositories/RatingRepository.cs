using System;
using Dapper;
using Movies.Application.Models;
using Movies.Application.Database;

namespace Movies.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public RatingRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.ExecuteAsync(new CommandDefinition("""
        DELETE FROM ratings
        WHERE movieid = @MovieId AND userid = @UserId
        """, new { MovieId = movieId, UserId = userId }, cancellationToken: cancellationToken));
        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var rating = await connection.QueryFirstOrDefaultAsync<float?>(new CommandDefinition("""
        SELECT round(avg(rating), 1) as rating
        FROM ratings
        WHERE movieid = @MovieId
        """, new { MovieId = movieId }, cancellationToken: cancellationToken));

        return rating;
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QueryFirstOrDefaultAsync(new CommandDefinition("""
        SELECT 
        round(avg(rating), 1),
        (select rating FROM ratings where movieid = @MovieId and userid = @UserId limit 1)
        FROM ratings 
        WHERE movieid = @MovieId
        """, new { MovieId = movieId, UserId = userId }, cancellationToken: cancellationToken));

        return (result?.rating, result?.userRating);
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QueryAsync<MovieRating>(new CommandDefinition("""
        SELECT r.rating, r.movieid, m.slug
        FROM ratings r
        INNER JOIN movies m ON r.movieid = m.id
        WHERE r.userid = @UserId
        """, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
        INSERT INTO ratings (movieid, rating, userid)
        VALUES (@MovieId, @Rating, @UserId)
        ON CONFLICT (movieid, userid) DO UPDATE SET rating = @Rating
        """, new { MovieId = movieId, Rating = rating, UserId = userId }, cancellationToken: cancellationToken));
        return result > 0;
    }
}
