using System;
using Dapper;

namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public DbInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        // Here you can execute any database initialization logic, such as creating tables, etc.

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS movies (
                id UUID PRIMARY KEY,
                title TEXT NOT NULL,
                slug TEXT NOT NULL UNIQUE,
                year_of_release INT NOT NULL
            );
            """);

        await connection.ExecuteAsync("""
            CREATE UNIQUE INDEX IF NOT EXISTS movies_slug_idx ON movies using btree(slug);
            """);

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS genres (
                movieId UUID REFERENCES movies(id),
                name TEXT NOT NULL
            );
            """);

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS ratings (
                userid UUID,
                movieId UUID REFERENCES movies(id),
                rating INTEGER NOT NULL,
                PRIMARY KEY (userid, movieId)
            );
            """);
    }
}
