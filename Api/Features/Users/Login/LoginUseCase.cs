using Api.Common;

namespace Api.Features.Users.Login;

internal sealed class LoginUseCase : ILoginUseCase
{
    public Task<LoginResponse> Handle(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    Task<Result<LoginResponse>> ILoginUseCase.Handle(LoginRequest request)
    {
        throw new NotImplementedException();
    }
}
