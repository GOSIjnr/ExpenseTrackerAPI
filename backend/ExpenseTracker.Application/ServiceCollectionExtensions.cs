using ExpenseTracker.Application.Configurations.Caching;
using ExpenseTracker.Application.Configurations.Security;
using ExpenseTracker.Application.Configurations.Seeders;
using ExpenseTracker.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplicationServices()
        {
            services.AddOptions<CacheTimeToLiveOptions>()
                .BindConfiguration(nameof(CacheTimeToLiveOptions));

            services.AddSingleton<IValidateOptions<CacheTimeToLiveOptions>, CacheTimeToLiveOptionsValidator>();

            services.AddOptions<SessionLifetimeOptions>()
                .BindConfiguration(nameof(SessionLifetimeOptions));

            services.AddSingleton<IValidateOptions<SessionLifetimeOptions>, SessionLifetimeOptionsValidator>();

            services.AddOptions<SuperAdminSeedOptions>()
                .BindConfiguration(nameof(SuperAdminSeedOptions));

            services.AddSingleton<IValidateOptions<SuperAdminSeedOptions>, SuperAdminSeedOptionsValidator>();

            services.AddScoped<SessionStateService>();
            services.AddScoped<UserAuthenticationStateService>();

            return services;
        }
    }
}
