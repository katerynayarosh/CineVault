using CineVault.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCineVaultDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO 1 Налаштувати DbContext для роботи з базою даних
        services.AddDbContext<CineVaultDbContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("CineVaultDb");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured");
            }

            options.UseSqlServer(connectionString);
        });

        return services;
    }

    public static bool IsLocal(this IWebHostEnvironment environment)
    {
        return environment.EnvironmentName == "Local";
    }
}
