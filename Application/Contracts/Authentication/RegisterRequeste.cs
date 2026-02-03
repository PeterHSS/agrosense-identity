namespace Application.DTOs.Authentication;

public record RegisterRequest(string Name, DateTime BirhDate, string Email, string Password);
