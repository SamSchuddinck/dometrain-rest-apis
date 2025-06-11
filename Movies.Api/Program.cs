using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using Movies.Api;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.Database;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddAuthentication(authConfig =>
{
    authConfig.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    authConfig.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    authConfig.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(tokenConfig =>
{
    tokenConfig.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true,
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthContstants.AdminUserPolicyName, policy =>
    {
        policy.RequireClaim(AuthContstants.AdminUserClaimName, "true");
    });

    options.AddPolicy(AuthContstants.TrusterMemberPolicyName, policy =>
    {
        policy.RequireAssertion(c =>
            c.User.HasClaim(AuthContstants.TrusterMemberClaimName, "true") ||
            c.User.HasClaim(AuthContstants.AdminUserClaimName, "true")
        );
    });
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplication();
builder.Services.AddDatabase(config["Database:ConnectionString"]!);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ValidationMappingMiddleware>();
app.MapControllers();

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();