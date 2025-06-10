using Dapper;
using Movies.Application.Models;
using Movies.Application.Database;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{

    private readonly IDbConnectionFactory _dbConnectionFactory;
    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition("""
        INSERT INTO movies (id, slug, title, year_of_release)
        VALUES (@Id, @Slug, @Title, @YearOfRelease)
        """, movie, cancellationToken: cancellationToken));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO genres (movieid, name)
                VALUES (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }, cancellationToken: cancellationToken));
            }
        }

        transaction.Commit();
        return result > 0;
    }

   

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions moviesOptions, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var orderClause = string.Empty;

        if (moviesOptions.SortField is not null)
        {
            orderClause = $""", movies.{moviesOptions.SortField} ORDER BY {moviesOptions.SortField} {(moviesOptions.SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}""";
        }

        var pageOffset = (moviesOptions.Page - 1) * moviesOptions.PageSize;
        
        var result = await connection.QueryAsync(new CommandDefinition($"""

        SELECT movies.*, string_agg(DISTINCT genres.name, ',') AS genres,
        ROUND(AVG(ratings.rating), 1) AS rating,
        myrating.rating AS userrating
        FROM movies
        LEFT JOIN genres genres ON movies.id = genres.movieid
        LEFT JOIN ratings ON movies.id = ratings.movieid
        LEFT JOIN ratings myrating ON movies.id = myrating.movieid AND myrating.userid = @userId
        WHERE (@title IS NULL OR movies.title ILIKE ('%' || @title || '%'))
        AND (@yearOfRelease IS NULL OR movies.year_of_release = @yearOfRelease)
        GROUP BY movies.id, userrating {orderClause}
        LIMIT @pageSize OFFSET @pageOffset
        """, new
        {
            userId = moviesOptions.UserId,
            title = moviesOptions.Title,
            yearOfRelease = moviesOptions.YearOfRelease,
            pageSize = moviesOptions.PageSize,
            pageOffset = pageOffset
        }, cancellationToken: cancellationToken));

        return result.Select(dbMovie => new Movie
        {
            Id = dbMovie.id,
            Title = dbMovie.title,
            YearOfRelease = dbMovie.year_of_release,
            Rating = (float?)dbMovie.rating,
            UserRating = (int?)dbMovie.userrating,
            Genres = Enumerable.ToList(dbMovie.genres.Split(','))
        });

    }

    public async Task<int> GetCountAsync(GetAllMoviesOptions moviesOptions, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        var result = await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
        SELECT COUNT(DISTINCT movies.id)
        FROM movies
        LEFT JOIN genres genres ON movies.id = genres.movieid
        WHERE (@title IS NULL OR movies.title ILIKE ('%' || @title || '%'))
        AND (@yearOfRelease IS NULL OR movies.year_of_release = @yearOfRelease)
        """, new
        {
            title = moviesOptions.Title,
            yearOfRelease = moviesOptions.YearOfRelease
        }, cancellationToken: cancellationToken));

        return result;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var dbMovie = await connection.QueryFirstOrDefaultAsync(new CommandDefinition("""
        SELECT m.*, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS userrating
        FROM movies m
        LEFT JOIN ratings r ON m.id = r.movieid
        LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @userId
        WHERE id = @Id
        GROUP BY id, userrating
        """, new { id, userId }, cancellationToken: cancellationToken));


        if (dbMovie is null)
        {
            return null;
        }

        var movie = new Movie{
            Id = dbMovie.id,
            Title = dbMovie.title,
            YearOfRelease = dbMovie.year_of_release,
            Rating = (float?)dbMovie.rating,
            UserRating = (int?)dbMovie.userrating,
            Genres = new List<string>()
        };

        var genres = await connection.QueryAsync<string>(new CommandDefinition("""
        SELECT name FROM genres WHERE movieid = @MovieId
        """, new { MovieId = movie.Id }, cancellationToken: cancellationToken));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default)
    {
       using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var dbMovie = await connection.QueryFirstOrDefaultAsync(new CommandDefinition("""
        SELECT m.*, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS userrating
        FROM movies m
        LEFT JOIN ratings r ON m.id = r.movieid
        LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @UserId
        WHERE slug = @Slug
        GROUP BY id, userrating
        """, new { Slug = slug, UserId = userId }, cancellationToken: cancellationToken));


        if (dbMovie is null)
        {
            return null;
        }

        var movie = new Movie{
            Id = dbMovie.id,
            Title = dbMovie.title,
            YearOfRelease = dbMovie.year_of_release,
            Genres = new List<string>(),
            Rating = (float?)dbMovie.rating,
            UserRating = (int?)dbMovie.userrating
        };

        var genres = await connection.QueryAsync<string>(new CommandDefinition("""
        SELECT name FROM genres WHERE movieid = @MovieId
        """, new { MovieId = movie.Id }, cancellationToken: cancellationToken));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
        return movie;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("""
        DELETE FROM genres WHERE movieid = @Id;
        """, new { Id = movie.Id }, cancellationToken: cancellationToken));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO genres (movieid, name)
            VALUES (@MovieId, @Name)
            """, new { MovieId = movie.Id, Name = genre }, cancellationToken: cancellationToken));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition("""
        UPDATE movies
        SET slug = @Slug, title = @Title, year_of_release = @YearOfRelease
        WHERE id = @Id
        """, movie, cancellationToken: cancellationToken));

        transaction.Commit();

        return result > 0;
    }

     public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("""
        DELETE FROM genres WHERE movieid = @Id
        """, new { Id = id }, cancellationToken: cancellationToken));

        var result = await connection.ExecuteAsync(new CommandDefinition("""
        DELETE FROM movies WHERE id = @Id
        """, new {id}, cancellationToken: cancellationToken));

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
        SELECT COUNT(1) FROM movies WHERE id = @id
        """, new { id }, cancellationToken: cancellationToken));
    }
}