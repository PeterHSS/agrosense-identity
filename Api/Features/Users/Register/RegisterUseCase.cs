using Api.Common;
using Api.Domain.Abstractions.UseCases;
using Api.Infrastructure.Persistence.Contexts;
using Api.Infrastructure.Providers.PasswordHasher;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Users.Register;

internal sealed class RegisterUseCase(IValidator<RegisterRequest> validator, UserDbContext context, IPasswordHasherProvider passwordHasher) : IUseCase<RegisterRequest>
{
    public async Task<Result> Handle(RegisterRequest request)
    {
        var validationResult = validator.Validate(request);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

            return Result.Failure(UserErrors.Validation(errors));
        }

        if (await context.Users.AnyAsync(user => user.Email == request.Email))
            return Result.Failure(UserErrors.UserAlreadyExists);

        var password = passwordHasher.Hash(request.Password);

        var user = request.ToUser(password);

        context.Users.Add(user);

        await context.SaveChangesAsync();

        return Result.Success();
    }
}
