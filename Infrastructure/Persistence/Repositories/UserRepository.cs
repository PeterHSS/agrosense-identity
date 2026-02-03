using Domain.Abstraction.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(UserDbContext dbContext) : IUserRepository
{
    public async Task Add(User user)
    {
        await dbContext.Users.AddAsync(user);
    }

    public async Task<bool> EmailExists(string email)
    {
        return await dbContext.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User?> GetByEmail(string email)
    {
        return await dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
    }
}
