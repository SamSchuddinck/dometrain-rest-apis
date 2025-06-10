public class Movie
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }

    public string Slug => GenerateSlug();

    public float? Rating { get; set; }

    public int? UserRating { get; set; }
    public required int YearOfRelease { get; set; }
    public required List<string> Genres { get; init; } = new();
    private string GenerateSlug()
    {
        return $"{Title.ToLowerInvariant().Replace(" ", "-")}-{YearOfRelease}";
    }
}