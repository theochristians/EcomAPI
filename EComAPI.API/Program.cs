using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using EComAPI.API.Authorization;
using EComAPI.API.Common;
using EComAPI.API.Common.Middleware;
using EComAPI.Application;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Infrastructure;
using EComAPI.Infrastructure.Auth.Persistence.Seeders;
using EComAPI.Infrastructure.Auth.Security;
using EComAPI.Infrastructure.Common.Persistence.Context;
using EComAPI.Infrastructure.Common.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// Core Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EComAPI",
        Version = "v1",
        Description = "E-Commerce API with Clean Architecture"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Rate Limiting (dinonaktifkan di environment Test agar integration tests tidak terkena batas)
if (!builder.Environment.IsEnvironment("Test"))
{
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail("Too many requests. Please try again later.");
        await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
    };

    options.AddPolicy("auth-register", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetClientIp(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth-login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetClientIp(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("email-verify-send", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetUserIdOrIp(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("email-verify-check", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetUserIdOrIp(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
} // end if (!Test)

// Application & Infrastructure
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// JWT Authentication
var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.Fail("Unauthorized");
                return context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        };
    });

// Permission Authorization
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddAuthorization();

var app = builder.Build();

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();
    var passwordHasher = services.GetRequiredService<IPasswordHasher>();

    try
    {
        Console.WriteLine("?? Starting database seeding...");
        await PermissionSeeder.SeedAsync(dbContext);
        await AdminSeeder.SeedAsync(dbContext, passwordHasher);
        Console.WriteLine("? Database seeding completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Seeding error: {ex.Message}");
        throw;
    }
}

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

// Console Output
Console.WriteLine("??????????????????????????????????????");
Console.WriteLine("?? EComAPI is running!");
Console.WriteLine("?? Swagger UI: https://localhost:7091/swagger");
Console.WriteLine("?? API Base:   https://localhost:7091/api");
Console.WriteLine("??????????????????????????????????????");

app.Run();

static string GetClientIp(HttpContext httpContext)
{
    return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

static string GetUserIdOrIp(HttpContext httpContext)
{
    var userId = httpContext.User.FindFirst("sub")?.Value ??
                 httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (!string.IsNullOrWhiteSpace(userId))
        return $"user:{userId}";

    return $"ip:{GetClientIp(httpContext)}";
}

// Expose Program class for WebApplicationFactory in integration tests
public partial class Program { }
