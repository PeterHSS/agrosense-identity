using Application.DTOs.Authentication;
using Application.Interfaces;
using Application.Mappers;
using Domain.Abstraction.Infrastructure;
using Domain.Abstraction.Repositories;
using FluentValidation;

namespace Application.Services;

internal sealed class AuthenticationService(
    IPasswordHasherProvider passwordHashProvider, 
    IUserRepository userRepository, 
    IValidator<RegisterRequest> registerValidator, 
    IJwtProvider jwtProvider) : IAuthenticationService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmail(request.Email) 
            ?? throw new UnauthorizedAccessException("Invalid email or password.");
        
        var isPasswordValid = passwordHashProvider.Verify(user.Password, request.Password);

        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = jwtProvider.Create(user);

        return new LoginResponse(token);
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        registerValidator.ValidateAndThrow(request);

        if (await userRepository.EmailExists(request.Email))
            throw new InvalidOperationException("Email already exists.");

        var hashedPassword = passwordHashProvider.Hash(request.Password);

        var user = request.ToEntity(hashedPassword);

        await userRepository.Add(user);

    }
}
