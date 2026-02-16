using FluentValidation;

namespace Api.Features.Users.Register;

internal sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .EmailAddress().WithMessage("O email fornecido não é válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter pelo menos 6 caracteres.")
            .Must(BeAValidPassword).WithMessage("A senha deve conter letras, números e caracteres especiais.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100).WithMessage("O nome não pode exceder 100 caracteres.");
    }

    private bool BeAValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return password.Any(char.IsLetter) &&
               password.Any(char.IsDigit) &&
               password.Any(c => !char.IsLetterOrDigit(c));
    }
}
