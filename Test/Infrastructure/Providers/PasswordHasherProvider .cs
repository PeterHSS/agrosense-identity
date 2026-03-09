using Api.Infrastructure.Providers.PasswordHasher;

namespace Test.Infrastructure.Providers;

public class PasswordHasherProviderTests
{
    private readonly PasswordHasherProvider _provider = new();

    // ─── Hash ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = _provider.Hash("senha123");

        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    [Fact]
    public void Hash_ReturnsExpectedFormat_HashDashSalt()
    {
        var hash = _provider.Hash("senha123");
        var parts = hash.Split('-');

        Assert.Equal(2, parts.Length);
        Assert.All(parts, p => Assert.False(string.IsNullOrWhiteSpace(p)));
    }

    [Fact]
    public void Hash_DoesNotStoreRawPassword()
    {
        var hash = _provider.Hash("senha_secreta");

        Assert.DoesNotContain("senha_secreta", hash);
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Salt is random, so identical passwords must produce different hashes
        var hash1 = _provider.Hash("senha123");
        var hash2 = _provider.Hash("senha123");

        Assert.NotEqual(hash1, hash2);
    }

    // ─── Verify ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hashed = _provider.Hash("senha123");

        Assert.True(_provider.Verify("senha123", hashed));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hashed = _provider.Hash("senha123");

        Assert.False(_provider.Verify("senha_errada", hashed));
    }

    [Fact]
    public void Verify_WithEmptyPassword_ReturnsFalse()
    {
        var hashed = _provider.Hash("senha123");

        Assert.False(_provider.Verify("", hashed));
    }

    [Fact]
    public void Verify_WithInvalidHashFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => _provider.Verify("senha123", "formato_invalido"));
    }

    [Fact]
    public void Verify_WithTamperedHash_ReturnsFalse()
    {
        var hashed = _provider.Hash("senha123");
        var parts = hashed.Split('-');
        var tampered = $"0000000000000000000000000000000000000000000000000000000000000000-{parts[1]}";

        Assert.False(_provider.Verify("senha123", tampered));
    }

    [Fact]
    public void Verify_IsCaseSensitive()
    {
        var hashed = _provider.Hash("Senha123");

        Assert.False(_provider.Verify("senha123", hashed));
    }

    // ─── Round-trip ───────────────────────────────────────────────────────────────

    [Fact]
    public void HashAndVerify_WorksWithSpecialCharacters()
    {
        const string password = "p@$$w0rd!#%&*()";
        var hashed = _provider.Hash(password);

        Assert.True(_provider.Verify(password, hashed));
    }

    [Fact]
    public void HashAndVerify_WorksWithLongPassword()
    {
        var password = new string('a', 512);
        var hashed = _provider.Hash(password);

        Assert.True(_provider.Verify(password, hashed));
    }
}