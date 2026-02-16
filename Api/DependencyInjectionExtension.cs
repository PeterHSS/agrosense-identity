using Api.Infrastructure.Persistence.Contexts;
using Api.Infrastructure.Providers.Jwt;
using Api.Infrastructure.Providers.PasswordHasher;
using Api.Infrastructure.Settings;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddContext(configuration);
        services.AddProviders();
        services.AddSettings(configuration);

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidators();

        return services;
    }
    public static void ApplyMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        using var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        context.Database.Migrate();
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjectionExtension).Assembly, ServiceLifetime.Scoped, includeInternalTypes: true);

        return services;
    }

    private static IServiceCollection AddProviders(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasherProvider, PasswordHasherProvider>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        return services;
    }

    private static IServiceCollection AddContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

        services.AddDbContext<UserDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }

    private static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value);

        return services;
    }
}
