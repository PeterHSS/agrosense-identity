using Api.Domain.Entities;

namespace Api.Infrastructure.Providers.Jwt;

public interface IJwtProvider
{
    string CreateToken(User user);
}
