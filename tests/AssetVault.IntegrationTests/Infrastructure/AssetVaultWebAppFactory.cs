using AssetVault.Application.Common.Interfaces;
using AssetVault.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace AssetVault.IntegrationTests.Infrastructure;

/// <summary>
/// Shared test fixture that owns a single Postgres Testcontainer for the lifetime of a test class.
/// Registered as <c>IClassFixture&lt;AssetVaultWebAppFactory&gt;</c>, so xUnit creates one instance
/// per test class and calls <see cref="InitializeAsync"/> / <see cref="DisposeAsync"/> once.
/// <para>
/// Per-test isolation is handled by <see cref="IntegrationTestBase"/>, which calls
/// <see cref="ResetDatabaseAsync"/> before every test method via its own <c>IAsyncLifetime</c>.
/// </para>
/// Lifecycle (per test class):
/// <list type="number">
///   <item>InitializeAsync — start container, run migrations, create Respawner checkpoint</item>
///   <item>ResetDatabaseAsync — called before each test to truncate user tables</item>
///   <item>DisposeAsync — stop container</item>
/// </list>
/// </summary>
public class AssetVaultWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private Respawner _respawner = default!;

    public string ConnectionString => _db.GetConnectionString();

    /// <summary>
    /// Called once when the test class starts. Starts the container, applies EF Core migrations,
    /// then creates a <see cref="Respawner"/> checkpoint that represents the clean post-migration state.
    /// The checkpoint is reused by every subsequent <see cref="ResetDatabaseAsync"/> call.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = [new Table("__EFMigrationsHistory")]
        });
    }

    /// <summary>
    /// Truncates all user tables back to the post-migration state using Respawn.
    /// Called by <see cref="IntegrationTestBase.InitializeAsync"/> before every test method.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
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
            options.UseNpgsql(ConnectionString));
    }

    private static void ReplaceStorageService(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStorageService));
        if (descriptor is not null) services.Remove(descriptor);

        services.AddScoped<IStorageService, FakeStorageService>();
    }

    /// <summary>Called once after all tests in the class have run. Stops the container.</summary>
    public new async Task DisposeAsync()
    {
        await _db.StopAsync();
        await base.DisposeAsync();
    }
}
