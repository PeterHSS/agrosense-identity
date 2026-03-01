using Api.Common;
using Api.Common.Middlewares;
using Api.Domain.Abstractions.UseCases;
using Api.Domain.Entities.Enums;
using Api.Features.Users.Login;
using Api.Features.Users.Register;
using Api.Infrastructure.Persistence.Contexts;
using Api.Infrastructure.Providers.Jwt;
using Api.Infrastructure.Providers.PasswordHasher;
using Api.Infrastructure.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddDependecyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddApplication();
        services.AddPresentation(configuration);
        return services;
    }

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();
        services.AddJwtAuthenticationAndAuthorization(configuration);
        services.AddHealthChecks();

        return services;
    }

    private static IServiceCollection AddJwtAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services
            .AddAuthorizationBuilder()
            .AddPolicy(Policies.AdministratorOnly, policy => policy.RequireRole(nameof(Role.Admin)));

        return services;
    }

    public static void ApplyMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        using var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        context.Database.Migrate();
    }

    private static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddContexts(configuration);
        services.AddProviders();
        services.AddSettings(configuration);

        return services;
    }

    private static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidators();
        services.AddUseCases();

        return services;
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUseCase<LoginRequest, LoginResponse>, LoginUseCase>();
        services.AddScoped<IUseCase<RegisterRequest>, RegisterUseCase>();

        return services;
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

    private static IServiceCollection AddContexts(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("ConnectionString not configured.");

        services.AddDbContext<UserDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }

    private static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSetting>(configuration.GetSection(JwtSetting.SectionName));

        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOptions<JwtSetting>>().Value);

        return services;
    }
}
