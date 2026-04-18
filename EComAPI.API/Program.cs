using Microsoft.AspNetCore.Authorization;
using EComAPI.API.Authorization;
using EComAPI.API.Common;
using EComAPI.API.Common.Middleware;
using EComAPI.API.DependencyInjection;
using EComAPI.API.Startup;
using EComAPI.API.Auth.BackgroundServices;
using EComAPI.Application;
using EComAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Core Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddApiSwagger();

// Rate Limiting (dinonaktifkan di environment Test agar integration tests tidak terkena batas)
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddApiRateLimiting(builder.Configuration);
}

// Application & Infrastructure
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddHostedService<TokenBlacklistCleanupHostedService>();
}

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Permission Authorization
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddAuthorization();

var app = builder.Build();

// Database Seeding
await app.SeedDatabaseAsync();

// ? MIDDLEWARE PIPELINE (ORDER MATTERS!)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EComAPI v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "EComAPI - Swagger UI";
    });
}

app.UseHttpsRedirection();

// ? 1. Exception handling first
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ? 2. Rate limiting
if (!app.Environment.IsEnvironment("Test"))
{
    app.UseRateLimiter();
}

// ? 3. Authentication (JWT validation)
app.UseAuthentication();

// ? 4. Authorization (permission check)
app.UseAuthorization();

// ? 5. Token blacklist check (AFTER authentication)
app.UseMiddleware<TokenBlacklistMiddleware>();

// ? 6. Controllers
app.MapControllers();

app.Run();

public partial class Program { }
