using Api.Domain.Entities;
using Api.Features.Users;
using Api.Features.Users.Login;
using Api.Infrastructure.Persistence.Contexts;
using Api.Infrastructure.Providers.Jwt;
using Api.Infrastructure.Providers.PasswordHasher;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Test.Features.Users;

public class LoginUseCaseTests
{
    private readonly UserDbContext _context;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IPasswordHasherProvider> _passwordHasherMock;
    private readonly LoginUseCase _useCase;

    public LoginUseCaseTests()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserDbContext(options);
        _jwtProviderMock = new Mock<IJwtProvider>();
        _passwordHasherMock = new Mock<IPasswordHasherProvider>();

        _useCase = new LoginUseCase(_context, _jwtProviderMock.Object, _passwordHasherMock.Object);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    private static LoginRequest BuildRequest(string email = "user@agro.com", string password = "senha123") =>
        new(email, password);

    private async Task<User> SeedUserAsync(string email = "user@agro.com", string hashedPassword = "hashed_senha123")
    {
        var user = new User("Produtor Teste", email, hashedPassword);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    // ─── User not found ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsUserNotExists()
    {
        // Arrange
        var request = BuildRequest(email: "naoexiste@agro.com");

        // Act
        var result = await _useCase.Handle(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.UserNotExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_DoesNotCallPasswordHasher()
    {
        // Arrange
        var request = BuildRequest(email: "naoexiste@agro.com");

        // Act
        await _useCase.Handle(request);

        // Assert
        _passwordHasherMock.Verify(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_DoesNotCallJwtProvider()
    {
        // Arrange
        var request = BuildRequest(email: "naoexiste@agro.com");

        // Act
        await _useCase.Handle(request);

        // Assert
        _jwtProviderMock.Verify(j => j.CreateToken(It.IsAny<User>()), Times.Never);
    }

    // ─── Invalid password ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ReturnsInvalidCredentials()
    {
        // Arrange
        await SeedUserAsync();
        _passwordHasherMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var request = BuildRequest(password: "senha_errada");

        // Act
        var result = await _useCase.Handle(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCredentials.Code, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_VerifiesAgainstStoredHash()
    {
        // Arrange
        var user = await SeedUserAsync(hashedPassword: "hash_armazenado");
        _passwordHasherMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var request = BuildRequest(password: "senha_errada");

        // Act
        await _useCase.Handle(request);

        // Assert
        _passwordHasherMock.Verify(p => p.Verify("senha_errada", "hash_armazenado"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_DoesNotCallJwtProvider()
    {
        // Arrange
        await SeedUserAsync();
        _passwordHasherMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        await _useCase.Handle(BuildRequest());

        // Assert
        _jwtProviderMock.Verify(j => j.CreateToken(It.IsAny<User>()), Times.Never);
    }

    // ─── Happy path ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsSuccess()
    {
        // Arrange
        var user = await SeedUserAsync();
        _passwordHasherMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtProviderMock.Setup(j => j.CreateToken(It.IsAny<User>())).Returns("jwt_token");

        // Act
        var result = await _useCase.Handle(BuildRequest());

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsTokenFromJwtProvider()
    {
        // Arrange
        await SeedUserAsync();
        _passwordHasherMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtProviderMock.Setup(j => j.CreateToken(It.IsAny<User>())).Returns("jwt_token_gerado");

        // Act
        var result = await _useCase.Handle(BuildRequest());

        // Assert
        Assert.Equal("jwt_token_gerado", result.Value.AccessToken);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_CallsJwtProviderWithCorrectUser()
    {
        // Arrange
        var user = await SeedUserAsync(email: "user@agro.com");
        _passwordHasherMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtProviderMock.Setup(j => j.CreateToken(It.IsAny<User>())).Returns("token");

        // Act
        await _useCase.Handle(BuildRequest());

        // Assert
        _jwtProviderMock.Verify(j => j.CreateToken(It.Is<User>(u => u.Id == user.Id)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_DoesNotMatchAnotherUsersCredentials()
    {
        // Arrange
        await SeedUserAsync(email: "user1@agro.com", hashedPassword: "hash1");
        await SeedUserAsync(email: "user2@agro.com", hashedPassword: "hash2");

        _passwordHasherMock
            .Setup(p => p.Verify("senha123", "hash1"))
            .Returns(true);

        _jwtProviderMock.Setup(j => j.CreateToken(It.IsAny<User>())).Returns("token");

        var request = BuildRequest(email: "user1@agro.com", password: "senha123");

        // Act
        var result = await _useCase.Handle(request);

        // Assert
        Assert.True(result.IsSuccess);
        _passwordHasherMock.Verify(p => p.Verify("senha123", "hash2"), Times.Never);
    }
}