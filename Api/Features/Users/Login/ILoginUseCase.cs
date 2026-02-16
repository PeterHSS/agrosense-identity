using Api.Common;

namespace Api.Features.Users.Login;

public interface ILoginUseCase
{
    Task<Result<LoginResponse>> Handle(LoginRequest request);
}
