using System.Security.Cryptography;
using Domain.Abstraction.Infrastructure;

namespace Infrastructure.Providers;

internal sealed class PasswordHasherProvider : IPasswordHasherProvider
{
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private const int SaltSize = 16;

    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool Verify(string password, string hashedPassword)
    {
        string[] parts = hashedPassword.Split('-');

        if (parts.Length != 2)
            throw new FormatException("Password with invalid hash format.");

        byte[] hash = Convert.FromHexString(parts[0]);

        byte[] salt = Convert.FromHexString(parts[1]);

        byte[] hashToVerify = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, hashToVerify);
    }
}
