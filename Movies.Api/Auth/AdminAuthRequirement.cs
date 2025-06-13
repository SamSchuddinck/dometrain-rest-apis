using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Movies.Api.Auth;

public class AdminAuthRequirement : IAuthorizationHandler, IAuthorizationRequirement
{
    private readonly string _apiKey;

public AdminAuthRequirement(string apiKey)
    {
        _apiKey = apiKey;
    }
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.HasClaim(AuthContstants.AdminUserClaimName, "true"))
        {
            context.Succeed(this);
            return Task.CompletedTask;
        }

        var httpContext = context.Resource as HttpContext;
        if (!httpContext.Request.Headers.TryGetValue(AuthContstants.ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (_apiKey != extractedApiKey)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var identity = (ClaimsIdentity)httpContext.User.Identity;
        identity.AddClaim(new Claim("userid", Guid.Parse("d8566de3-b1a6-4a9b-b842-8e3887a82e43").ToString()));
        context.Succeed(this);
        return Task.CompletedTask;
    }
}
