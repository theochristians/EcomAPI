using EComAPI.API.Authorization;
using EComAPI.API.Common;
using EComAPI.API.Common.Identity;
using EComAPI.API.Common.Middlewares;
using EComAPI.Application;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Infrastructure;
using EComAPI.Infrastructure.Auth.Authorization;
using EComAPI.Infrastructure.Auth.Persistence.Seeders;
using EComAPI.Infrastructure.Auth.Security;
using EComAPI.Infrastructure.Common.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Core Services
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// =======================
// Swagger
// =======================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EComAPI",
        Version = "v1",
        Description = "E-Commerce Clean Architecture API"
    });

    c.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// =======================
// Application & Infrastructure
// =======================
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// =======================
// JWT Authentication
// =======================
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
                Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.Fail("Unauthorized");
                return context.Response.WriteAsync(
                    JsonSerializer.Serialize(response));
            }
        };
    });

// =======================
// Permission Authorization (INI INTI FIX)
// =======================
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddAuthorization();

// =======================
// BUILD APP
// =======================
var app = builder.Build();

// =======================
// Seed Permissions
// =======================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await PermissionSeeder.SeedAsync(dbContext);
}

// =======================
// Pipeline
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
