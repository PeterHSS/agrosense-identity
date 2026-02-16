using Api.Common;

namespace Api.Features.Users.Register;

public interface IRegisterUseCase
{
    Task<Result> Handle(RegisterRequest request);
}
