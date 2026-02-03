using Application.DTOs.Authentication;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IResult> LoginAsync([FromBody] LoginRequest request)
    {
        var response = await authenticationService.LoginAsync(request);

        return Results.Ok(response);
    }

    [HttpPost("register")]
    public async Task<IResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        await authenticationService.RegisterAsync(request);

        return Results.Created();
    }
}
