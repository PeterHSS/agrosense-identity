using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class DatabaseExtension
{
    public static void ApplyMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        using var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        context.Database.Migrate();
    }
}
