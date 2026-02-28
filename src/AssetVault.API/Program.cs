using AssetVault.API.Extensions;
using AssetVault.API.Middleware;
using AssetVault.Application.Extensions;
using AssetVault.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddApiDocs();

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseApiDocs();

app.UseCors("Frontend");

// Global exception handling for consistent error responses
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Custom middleware to set the current user context for each request
app.UseMiddleware<UserProfileMiddleware>();

app.MapControllers();
app.Run();

// Expose the Program class for integration testing purposes
public partial class Program { }
