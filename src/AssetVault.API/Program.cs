using AssetVault.Application.Common.Behaviour;
using AssetVault.Infrastructure.Extensions;
using AssetVault.Infrastructure.Storage;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Application
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssetVault.Application.AssemblyMarker).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(AssetVault.Application.AssemblyMarker).Assembly);

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// API
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Scalar UI at /scalar/v1
}

var s3Options = app.Services.GetRequiredService<IOptions<S3StorageOptions>>().Value;
Console.WriteLine($"UseHttp: {s3Options.UseHttp}");
Console.WriteLine($"ServiceUrl: {s3Options.ServiceUrl}");

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// Expose the Program class for integration testing purposes
public partial class Program { }