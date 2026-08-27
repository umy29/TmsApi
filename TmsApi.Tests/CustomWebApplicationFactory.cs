using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsASecretKeyForTestingPurposesOnly123456!",
                ["Jwt:Issuer"] = "TmsTestIssuer",
                ["Jwt:Audience"] = "TmsTestAudience",
                ["ConnectionStrings:TmsDatabase"] = "Host=localhost;Database=TmsTestDb;Username=postgres;Password=test",
                ["Payments:ApiKey"] = "test-key",
                ["Payments:BaseUrl"] = "https://test.payments.local"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove all EF Core related registrations
            var toRemove = services
                .Where(d =>
                    (d.ServiceType.FullName ?? "").Contains("TmsDbContext") ||
                    (d.ServiceType.FullName ?? "").Contains("DbContextOptions") ||
                    (d.ImplementationType?.FullName ?? "").Contains("TmsDbContext") ||
                    (d.ServiceType.FullName ?? "").Contains("IDbContextFactory"))
                .ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            // Remove NpgSql health check to avoid connection string requirement
            var healthCheckToRemove = services
                .Where(d => d.ServiceType == typeof(IHealthCheck) ||
                           (d.ImplementationType?.FullName ?? "").Contains("Npg") ||
                           (d.ImplementationType?.FullName ?? "").Contains("Postgres"))
                .ToList();
            foreach (var d in healthCheckToRemove)
                services.Remove(d);

            var dbName = "TmsIntegrationTest_" + Guid.NewGuid().ToString("N");

            services.AddDbContext<TmsDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddDbContextFactory<TmsDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddScoped<TmsDbContext>(sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<TmsDbContext>>();
                return factory.CreateDbContext();
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseServiceProviderFactory(new DefaultServiceProviderFactory(
            new ServiceProviderOptions
            {
                ValidateScopes = false,
                ValidateOnBuild = false
            }));
        return base.CreateHost(builder);
    }
}
