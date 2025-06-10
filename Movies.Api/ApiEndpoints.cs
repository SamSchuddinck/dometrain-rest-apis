using System;

namespace Movies.Api;

public static class ApiEndpoints
{
    private const string ApiBasePath = "api";
    public static class Movies
    {
        private const string BasePath = $"{ApiBasePath}/movies";
        public const string Create = $"{BasePath}";
        public const string GetAll = BasePath;
        public const string Get = $"{BasePath}/{{idOrSlug}}";

        public const string Update = $"{BasePath}/{{id:guid}}";

        public const string Delete = $"{BasePath}/{{id:guid}}";

        public const string Rate = $"{BasePath}/{{id:guid}}/ratings";
        public const string DeleteRating = $"{BasePath}/{{id:guid}}/ratings";
    }

    public static class Ratings
    {
        public const string BasePath = $"{ApiBasePath}/ratings";
        public const string GetUserRatings = $"{BasePath}/me/";
    }
}
