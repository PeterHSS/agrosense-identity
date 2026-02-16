using Api.Common;

namespace Api.Features.Users;

public static class UserErrors
{
    public static readonly Error UserAlreadyExists = new("UserAlreadyExists", "A user with the provided email already exists.");
    public static readonly Error UserNotExists = new("UserNotExists", "A user with the provided email does not exist.");
    public static readonly Error InvalidCredentials = new("InvalidCredentials", "The provided credentials are invalid.");
    public static Error Validation(IEnumerable<string> errors) => new("Validation", string.Join(';', errors));
}
