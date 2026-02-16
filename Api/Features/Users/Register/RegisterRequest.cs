namespace Api.Features.Users.Register;

public record RegisterRequest(string Name, DateTime BirhDate, string Email, string Password);
