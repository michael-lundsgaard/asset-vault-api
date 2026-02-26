using AssetVault.API.Extensions;
using AssetVault.Application.Extensions;
using AssetVault.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddApiDocs();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseApiDocs();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Expose the Program class for integration testing purposes
public partial class Program { }