using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Persistence;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPersistenceServices(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("Database");

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(configuration));

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString)
                    .EnableSensitiveDataLogging(false)
                    .UseSnakeCaseNamingConvention()
            );

            return services;
        }
    }
}
