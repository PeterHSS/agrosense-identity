using Api.Common;
using Api.Infrastructure.Persistence.Contexts;
using Api.Infrastructure.Providers.Jwt;
using Api.Infrastructure.Providers.PasswordHasher;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Users.Login;

internal sealed class LoginUseCase(UserDbContext context, IJwtProvider jwtProvider, IPasswordHasherProvider passwordHasher) : ILoginUseCase
{
    public async Task<Result<LoginResponse>> Handle(LoginRequest request)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            return Result<LoginResponse>.Failure(UserErrors.UserNotExists);

        if (!passwordHasher.Verify(request.Password, user.Password))
            return Result<LoginResponse>.Failure(UserErrors.InvalidCredentials);

        var token = jwtProvider.CreateToken(user);

        return Result<LoginResponse>.Success(new LoginResponse(token));
    }
}
