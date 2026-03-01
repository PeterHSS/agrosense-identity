using Api.Common;

namespace Api.Features.Users;

public static class UserErrors
{
    public static readonly Error UserAlreadyExists = new("User.UserAlreadyExists", "A user with the provided email already exists.");
    public static readonly Error UserNotExists = new("User.UserNotExists", "A user with the provided email does not exist.");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "The provided credentials are invalid.");
    public static Error Validation(IEnumerable<string> errors) => new("User.Validation", string.Join(';', errors));
}
