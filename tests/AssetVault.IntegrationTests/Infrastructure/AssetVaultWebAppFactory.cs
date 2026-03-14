using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;

namespace AssetVault.IntegrationTests.Infrastructure;

public class AssetVaultWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ReplaceDbContext(services);
            ReplaceStorageService(services);
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    private void ReplaceDbContext(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
        if (descriptor is not null) services.Remove(descriptor);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(_db.GetConnectionString()));
    }

    private static void ReplaceStorageService(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStorageService));
        if (descriptor is not null) services.Remove(descriptor);

        services.AddScoped<IStorageService, FakeStorageService>();
    }

    public new async Task DisposeAsync()
    {
        await _db.StopAsync();
        await base.DisposeAsync();
    }
}

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    // Expects: Authorization: Test {userId}:{email}
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var value = authHeader.ToString();
        if (!value.StartsWith($"{SchemeName} "))
            return Task.FromResult(AuthenticateResult.NoResult());

        var payload = value[$"{SchemeName} ".Length..];
        var separatorIndex = payload.IndexOf(':');
        if (separatorIndex < 0)
            return Task.FromResult(AuthenticateResult.Fail("Invalid test auth format. Expected: Test {userId}:{email}"));

        var userId = payload[..separatorIndex];
        var email = payload[(separatorIndex + 1)..];

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class FakeStorageService : IStorageService
{
    public Task<PresignedUploadResult> GenerateUploadUrlAsync(
        Guid assetId, string fileName, string contentType, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PresignedUploadResult(
            $"https://fake-storage.test/upload/{assetId}/{fileName}",
            $"uploads/{assetId}/{fileName}",
            DateTime.UtcNow.AddMinutes(15)));

    public Task<PresignedDownloadResult> GenerateDownloadUrlAsync(
        string storagePath, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PresignedDownloadResult(
            $"https://fake-storage.test/download/{storagePath}",
            DateTime.UtcNow.AddHours(1)));

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
