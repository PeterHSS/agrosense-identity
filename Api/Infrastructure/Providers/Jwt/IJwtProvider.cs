using Api.Domain.Entities;

namespace Api.Infrastructure.Providers.Jwt;

public interface IJwtProvider
{
    string Create(User user);
}
