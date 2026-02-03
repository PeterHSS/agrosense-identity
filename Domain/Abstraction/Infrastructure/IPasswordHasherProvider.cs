namespace Domain.Abstraction.Infrastructure;

public interface IPasswordHasherProvider
{
    string Hash(string password);
    bool Verify(string hashedPassword, string providedPassword);
}
