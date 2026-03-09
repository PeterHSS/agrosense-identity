using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Domain.Entities;
using Api.Domain.Entities.Enums;
using Api.Infrastructure.Providers.Jwt;
using Api.Infrastructure.Settings;

namespace Test.Infrastructure.Providers;

public class JwtProviderTests
{
    private static JwtProvider BuildProvider(
        string secret = "super_secret_key_for_testing_purposes_only_32chars!",
        string issuer = "agrosense",
        string audience = "agrosense-users",
        int expirationInMinutes = 60)
    {
        var settings = new JwtSetting
        {
            Secret = secret,
            Issuer = issuer,
            Audience = audience,
            ExpirationInMinutes = expirationInMinutes
        };

        return new JwtProvider(settings);
    }

    private static User BuildUser(string name = "Produtor", string email = "user@agro.com", string password = "hashed")
        => new(name, email, password);

    private static JwtSecurityToken DecodeToken(string token)
        => new JwtSecurityTokenHandler().ReadJwtToken(token);

    // ─── Token structure ──────────────────────────────────────────────────────────

    [Fact]
    public void CreateToken_ReturnsNonEmptyString()
    {
        // Arrange
        var provider = BuildProvider();
        var user = BuildUser();

        // Act
        var token = provider.CreateToken(user);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void CreateToken_ReturnsValidJwtFormat()
    {
        // Arrange
        var provider = BuildProvider();
        var user = BuildUser();

        // Act
        var token = provider.CreateToken(user);

        // Assert — JWT has exactly 3 parts separated by dots
        Assert.Equal(3, token.Split('.').Length);
    }

    // ─── Claims ───────────────────────────────────────────────────────────────────

    [Fact]
    public void CreateToken_ContainsUserIdClaim()
    {
        // Arrange
        var provider = BuildProvider();
        var user = BuildUser();

        // Act
        var decoded = DecodeToken(provider.CreateToken(user));

        // Assert
        // ClaimTypes.NameIdentifier is serialized with its full URI in JWT,
        // so we search by the expected value instead.
        var claim = decoded.Claims.FirstOrDefault(c => c.Value == user.Id.ToString());
        Assert.NotNull(claim);
    }

    [Fact]
    public void CreateToken_ContainsEmailClaim()
    {
        // Arrange
        var provider = BuildProvider();
        var user = BuildUser(email: "user@agro.com");

        // Act
        var decoded = DecodeToken(provider.CreateToken(user));

        // Assert
        var claim = decoded.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.NotNull(claim);
        Assert.Equal("user@agro.com", claim.Value);
    }

    [Fact]
    public void CreateToken_ContainsRoleClaim()
    {
        // Arrange
        var provider = BuildProvider();
        var user = BuildUser();

        // Act
        var decoded = DecodeToken(provider.CreateToken(user));

        // Assert
        // ClaimTypes.Role is serialized with its full URI name in JWT,
        // so we search by the expected value instead of the claim type.
        var claim = decoded.Claims.FirstOrDefault(c => c.Value == user.Role.ToString());
        Assert.NotNull(claim);
    }

    // ─── Issuer & Audience ────────────────────────────────────────────────────────

    [Fact]
    public void CreateToken_ContainsCorrectIssuer()
    {
        // Arrange
        var provider = BuildProvider(issuer: "meu-issuer");
        var user = BuildUser();

        // Act
        var decoded = DecodeToken(provider.CreateToken(user));

        // Assert
        Assert.Equal("meu-issuer", decoded.Issuer);
    }

    [Fact]
    public void CreateToken_ContainsCorrectAudience()
    {
        // Arrange
        var provider = BuildProvider(audience: "meu-audience");
        var user = BuildUser();

        // Act
        var decoded = DecodeToken(provider.CreateToken(user));

        // Assert
        Assert.Contains("meu-audience", decoded.Audiences);
    }

    // ─── Expiration ───────────────────────────────────────────────────────────────

    [Fact]
    public void CreateToken_ExpiresAfterConfiguredMinutes()
    {
        // Arrange
        var provider = BuildProvider(expirationInMinutes: 30);
        var user = BuildUser();
        var before = DateTime.UtcNow.AddMinutes(29);
        var after = DateTime.UtcNow.AddMinutes(31);

        // Act
        var decoded = DecodeToken(provider.CreateToken(user));

        // Assert
        Assert.InRange(decoded.ValidTo, before, after);
    }

    [Fact]
    public void CreateToken_IsNotExpiredWhenJustCreated()
    {
        // Arrange
        var provider = BuildProvider(expirationInMinutes: 60);
        var user = BuildUser();

        // Act
        var decoded = DecodeToken(provider.CreateToken(user));

        // Assert
        Assert.True(decoded.ValidTo > DateTime.UtcNow);
    }

    // ─── Signature ────────────────────────────────────────────────────────────────

    [Fact]
    public void CreateToken_UsesHmacSha256Algorithm()
    {
        // Arrange
        var provider = BuildProvider();
        var user = BuildUser();

        // Act
        var decoded = DecodeToken(provider.CreateToken(user));

        // Assert
        Assert.Equal("HS256", decoded.Header.Alg);
    }
}