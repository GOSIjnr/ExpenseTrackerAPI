using System.Reflection;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Infrastructure.CQRS.Messaging;
using ExpenseTracker.Infrastructure.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ExpenseTracker.Infrastructure.CQRS.Decorators;
using ExpenseTracker.Application.CQRS.Decorators;
using Microsoft.Extensions.Options;
using ExpenseTracker.Infrastructure.Configurations.Security.Hashing;
using ExpenseTracker.Infrastructure.Configurations.Security.DataEncryption;
using ExpenseTracker.Application.Constants.Services;

namespace ExpenseTracker.Infrastructure;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructureServices(params Assembly[] assembliesToScan)
        {
            services.AddOptions<DataEncryptionOptions>()
                .BindConfiguration(nameof(DataEncryptionOptions));

            services.AddSingleton<IValidateOptions<DataEncryptionOptions>, DataEncryptionOptionsValidator>();
            services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();

            services.AddOptions<HashingOptions>()
                .BindConfiguration(nameof(HashingOptions));

            services.AddSingleton<IValidateOptions<HashingOptions>, HashingOptionsValidator>();
            services.AddKeyedSingleton<IHashService, EmailHashService>(HashPurpose.Email);
            services.AddKeyedSingleton<IHashService, PasswordHashService>(HashPurpose.Password);

            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1024;
            });

            services.AddSingleton<ICacheService, InMemoryCacheService>();

            services.AddCqrsWithValidation(assembliesToScan);

            return services;
        }

        private void AddCqrsWithValidation(Assembly[] assembliesToScan)
        {
            services.AddScoped<IMediator, Mediator>();

            foreach (Assembly assembly in assembliesToScan)
            {
                services.Scan(scan => scan
                    .FromAssemblies(assembly)
                    .AddClasses(classes => classes.AssignableTo(typeof(IHandler<,>)), publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                );

                services.Scan(scan => scan
                    .FromAssemblies(assembly)
                    .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)), publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                );
            }

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        }
    }
}
