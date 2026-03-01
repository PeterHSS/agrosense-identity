using Api.Domain.Abstractions.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Users.Register;

[ApiController]
[Route("api/users")]
public class RegisterController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] RegisterRequest request, [FromServices] IUseCase<RegisterRequest> useCase)
    {
        var result = await useCase.Handle(request);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.Created();
    }
}
