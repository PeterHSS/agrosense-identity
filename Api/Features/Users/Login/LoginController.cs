using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Users.Login;

[ApiController]
[Route("api/users")]
public class LoginController : ControllerBase
{
    public async Task<IResult> Handle([FromServices] ILoginUseCase useCase, [FromBody] LoginRequest request)
    {
        var response = await useCase.Handle(request);

        if (response.IsFailure) 
            return Results.BadRequest(response.Error);

        return Results.Ok(response.Value);
    }
}
