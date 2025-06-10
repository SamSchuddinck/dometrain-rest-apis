using System;

namespace Movies.Api.Auth;

public static class IdentityExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var userId = context.User.Claims.SingleOrDefault(claims => claims.Type == "userid");

        if(Guid.TryParse(userId?.Value, out var userGuid))
        {
            return userGuid;
        }
        return null;
    }
    
}
