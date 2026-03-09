using Api.Domain.Entities;
using Api.Features.Users;
using Api.Features.Users.Register;
using Api.Infrastructure.Persistence.Contexts;
using Api.Infrastructure.Providers.PasswordHasher;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Test.Features.Users;

public class RegisterUseCaseTests
{
    private readonly UserDbContext _context;
    private readonly Mock<IValidator<RegisterRequest>> _validatorMock;
    private readonly Mock<IPasswordHasherProvider> _passwordHasherMock;
    private readonly RegisterUseCase _useCase;

    public RegisterUseCaseTests()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserDbContext(options);
        _validatorMock = new Mock<IValidator<RegisterRequest>>();
        _passwordHasherMock = new Mock<IPasswordHasherProvider>();

        _useCase = new RegisterUseCase(_validatorMock.Object, _context, _passwordHasherMock.Object);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    private void SetupValidRequest() =>
        _validatorMock
            .Setup(v => v.Validate(It.IsAny<RegisterRequest>()))
            .Returns(new ValidationResult());

    private void SetupInvalidRequest(params string[] errors) =>
        _validatorMock
            .Setup(v => v.Validate(It.IsAny<RegisterRequest>()))
            .Returns(new ValidationResult(errors.Select(e => new ValidationFailure("Field", e))));

    private static RegisterRequest BuildRequest(string email = "user@agro.com", string password = "senha123") =>
        new("Produtor Teste", email, password);

    private async Task SeedUserAsync(string email = "user@agro.com")
    {
        _context.Users.Add(new User("Existente", email, "hashed"));
        await _context.SaveChangesAsync();
    }

    // ─── Validation failures ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsFailureWithErrors()
    {
        // Arrange
        SetupInvalidRequest("Name is required", "Email is invalid");

        // Act
        var result = await _useCase.Handle(BuildRequest());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.Validation(["Name is required", "Email is invalid"]).Code, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_DoesNotPersistUser()
    {
        // Arrange
        SetupInvalidRequest("Name is required");

        // Act
        await _useCase.Handle(BuildRequest());

        // Assert
        Assert.Empty(_context.Users);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_DoesNotCallPasswordHasher()
    {
        // Arrange
        SetupInvalidRequest("Name is required");

        // Act
        await _useCase.Handle(BuildRequest());

        // Assert
        _passwordHasherMock.Verify(p => p.Hash(It.IsAny<string>()), Times.Never);
    }

    // ─── Email already exists ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsUserAlreadyExists()
    {
        // Arrange
        SetupValidRequest();
        await SeedUserAsync(email: "user@agro.com");

        // Act
        var result = await _useCase.Handle(BuildRequest(email: "user@agro.com"));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.UserAlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_DoesNotCallPasswordHasher()
    {
        // Arrange
        SetupValidRequest();
        await SeedUserAsync(email: "user@agro.com");

        // Act
        await _useCase.Handle(BuildRequest(email: "user@agro.com"));

        // Assert
        _passwordHasherMock.Verify(p => p.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_DoesNotCreateDuplicateUser()
    {
        // Arrange
        SetupValidRequest();
        await SeedUserAsync(email: "user@agro.com");

        // Act
        await _useCase.Handle(BuildRequest(email: "user@agro.com"));

        // Assert
        Assert.Single(_context.Users);
    }

    // ─── Happy path ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenEverythingValid_ReturnsSuccess()
    {
        // Arrange
        SetupValidRequest();
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed_senha");

        // Act
        var result = await _useCase.Handle(BuildRequest());

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenEverythingValid_PersistsUser()
    {
        // Arrange
        SetupValidRequest();
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed_senha");

        // Act
        await _useCase.Handle(BuildRequest(email: "novo@agro.com"));

        // Assert
        Assert.Single(_context.Users);
        Assert.Equal("novo@agro.com", _context.Users.Single().Email);
    }

    [Fact]
    public async Task Handle_WhenEverythingValid_HashesPasswordBeforePersisting()
    {
        // Arrange
        SetupValidRequest();
        _passwordHasherMock.Setup(p => p.Hash("senha123")).Returns("hashed_senha123");

        // Act
        await _useCase.Handle(BuildRequest(password: "senha123"));

        // Assert
        var user = _context.Users.Single();
        Assert.Equal("hashed_senha123", user.Password);
    }

    [Fact]
    public async Task Handle_WhenEverythingValid_DoesNotStoreRawPassword()
    {
        // Arrange
        SetupValidRequest();
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed_senha");

        // Act
        await _useCase.Handle(BuildRequest(password: "senha_secreta"));

        // Assert
        var user = _context.Users.Single();
        Assert.NotEqual("senha_secreta", user.Password);
    }

    [Fact]
    public async Task Handle_WhenEverythingValid_CallsHasherOnce()
    {
        // Arrange
        SetupValidRequest();
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed");

        // Act
        await _useCase.Handle(BuildRequest());

        // Assert
        _passwordHasherMock.Verify(p => p.Hash(It.IsAny<string>()), Times.Once);
    }
}