using Domain.Entities;

namespace Domain.Abstraction.Infrastructure;

public interface IJwtProvider
{
    string Create(User user);
}
