namespace Api.Infrastructure.Providers.PasswordHasher;

public interface IPasswordHasherProvider
{
    string Hash(string password);
    bool Verify(string hashedPassword, string providedPassword);
}
