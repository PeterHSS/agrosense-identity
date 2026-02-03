using Domain.Entities;

namespace Domain.Abstraction.Repositories;

public interface IUserRepository
{
    Task<bool> EmailExists(string email);
    Task Add(User user);
    Task<User?> GetByEmail(string email);
}
