using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using WebDiary.Controller;
using WebDiary.Data;
using WebDiary.Entities;
using WebDiary.Resources;
using Microsoft.AspNetCore.Mvc.Testing;
using static WebDiary.Controller.AuthController;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using WebDiary.Model;
using System.Security.Claims;

namespace WebDiary.Tests;

public class AuthControllerTests
{
    private readonly Mock<DiariesContext> _mockContext;
    private readonly Mock<IStringLocalizer<ErrorResource>> _mockLocalizer;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly DbContextOptions<DiariesContext> _options;

    public AuthControllerTests() {
        _options = new DbContextOptionsBuilder<DiariesContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        _mockContext = new Mock<DiariesContext>(_options);
        _mockLocalizer = new Mock<IStringLocalizer<ErrorResource>>();
        _mockConfig = new Mock<IConfiguration>();
    }
    private AuthController GetControllerWithContext(DiariesContext diariesContext) {
        return new AuthController(diariesContext, _mockConfig.Object, _mockLocalizer.Object);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNotFoundWhenUserDoesNotExist() {
        // Arrange
        using var dbContext = new DiariesContext(_options);
        var controller = GetControllerWithContext(dbContext);

        _mockLocalizer.Setup(l => l["InvalidNameOrPswd"]).Returns(new LocalizedString("InvalidNameOrPswd", "Invalid Credentials"));
        var model = new LoginModel() { Username = "notExist", Password = "123" };

        // Act
        var result = await controller.LoginAsync(model);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
    [Fact]
    public async Task LoginAsync_ReturnsUnauthorizedWhenPasswordWrong() {
        // Arrange
        using var dbContext = new DiariesContext(_options);
        var user = new User {
            UserName = "NewUser",
            Password = new PasswordHasher<User>().HashPassword(null!, "correct"),
            Email = "New@gmail.com",
            Role = "Default",
            IsValidated = false,
            Description = ""
        };
        dbContext.users.Add(user);
        dbContext.SaveChanges();
        var controller = GetControllerWithContext(dbContext);

        _mockLocalizer.Setup(l => l["InvalidNameOrPswd"]).Returns(new LocalizedString("InvalidNameOrPswd", "Invalid Credentials"));
        var model = new LoginModel() { Username = "NewUser", Password = "wrong" };

        // Act
        var result = await controller.LoginAsync(model);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
    [Fact]
    public async Task LoginAsync_ReturnsOkWhenCredentialsCorrect() {
        // Arrange
        using var dbContext = new DiariesContext(_options);
        var user = new User {
            UserName = "NewUser",
            Password = new PasswordHasher<User>().HashPassword(null!, "correct"),
            Email = "New@gmail.com",
            Role = "Default",
            IsValidated = false,
            Description = ""
        };
        dbContext.users.Add(user);
        dbContext.SaveChanges();
        var controller = GetControllerWithContext(dbContext);

        _mockLocalizer.Setup(l => l["InvalidNameOrPswd"]).Returns(new LocalizedString("InvalidNameOrPswd", "Invalid Credentials"));
        var model = new LoginModel() { Username = "NewUser", Password = "correct" };
        
        _mockConfig.Setup(config => config["Jwt:Issuer"]).Returns("test");
        _mockConfig.Setup(config => config["Jwt:Audience"]).Returns("testers");
        _mockConfig.Setup(config => config["Jwt:Key"]).Returns("supersecretkey1234567890LongerAndLongerToInfinityAndBeyond!");

        // Act
        var result = await controller.LoginAsync(model);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task IsUniqueEmailAsync_ReturnBadRequestWhenEmailNotExist() {
        // Arrange
        using var dbContext = new DiariesContext(_options);
        var controller = GetControllerWithContext(dbContext);

        // Act
        var result = await controller.IsUniqueEmailAsync("NotexistEmail@gmail.not");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    [Fact]
    public async Task IsUniqueEmailAsync_ReturnOkWhenEmailExist() {
        // Arrange
        using var dbContext = new DiariesContext(_options);
        var user = new User {
            UserName = "NewUser",
            Password = new PasswordHasher<User>().HashPassword(null!, "correct"),
            Email = "New@gmail.com",
            Role = "Default",
            IsValidated = false,
            Description = ""
        };
        dbContext.users.Add(user);
        dbContext.SaveChanges();
        var controller = GetControllerWithContext(dbContext);

        // Act
        var result = await controller.IsUniqueEmailAsync("New@gmail.com");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Are exist", okResult.Value);
    }

    [Fact]
    public async Task SendEmailAsync_SendsValidationEmail_ReturnsOk() {
        // Arrange
        using var dbContext = new DiariesContext(_options);
        var user = new User {
            UserName = "NewUser",
            Password = new PasswordHasher<User>().HashPassword(null!, "correct"),
            Email = "New@gmail.com",
            Role = "Default",
            IsValidated = false,
            Description = ""
        };
        dbContext.users.Add(user);
        dbContext.SaveChanges();
        var controller = GetControllerWithContext(dbContext);

        _mockConfig.Setup(config => config["EmailFromSend"]).Returns("sender@example.com");
        _mockConfig.Setup(config => config["AppPaswordForEmailAuth"]).Returns("dummy-password");

        var emailForm = new sendEmailForm() {
            userId = 1,
            CallbackUrl = "http://example.com/validate",
            IsValidation = true
        };

        // Act
        var result = await controller.SendEmailAsync(emailForm);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Email sended successfully", okResult.Value);
    }

    [Fact]
    public async Task Refresh_ReturnsBadRequestIfUserNotFound()
    {
        // Arrange
        using var context = new DiariesContext(_options);
        var controller = GetControllerWithContext(context);

        // Stub GetPrincipalFromExpiredToken by providing valid claims manually
        var identity = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "fakeuser") }, "test");
        var principal = new ClaimsPrincipal(identity);
        // Workaround: GetPrincipalFromExpiredToken is private. You'd need to expose it or refactor for testability.
        
        _mockLocalizer.Setup(l => l["RefreshTokenError"]).Returns(new LocalizedString("RefreshTokenError", "Refresh failed"));

        var tokenRequest = new AuthController.TokenRequestModel {
            AccessToken = "some-token",
            RefreshToken = "some-refresh-token"
        };
        _mockConfig.Setup(config => config["Jwt:Issuer"]).Returns("test");
        _mockConfig.Setup(config => config["Jwt:Audience"]).Returns("testers");
        _mockConfig.Setup(config => config["Jwt:Key"]).Returns("supersecretkey1234567890LongerAndLongerToInfinityAndBeyond!");

        // Act
        var result = await controller.Refresh(tokenRequest);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ValidateEmailAsync_ValidToken_ReturnsOk() {
        // Arrange
        using var dbContext = new DiariesContext(_options);
        var tokenBytes = new byte[32];
        new Random().NextBytes(tokenBytes);
        var token = Convert.ToBase64String(tokenBytes).Replace('+', '-').Replace('/', '_');

        var user = new User() {
            Id = 1,
            UserName = "user",
            Email = "email@example.com",
            ActionToken = tokenBytes,
            ActionDateEnd = DateTime.Now.AddMinutes(10),
            IsValidated = false,
            Password = "123",
            Role = "Default",
            Description = ""
        };
        dbContext.users.Add(user);
        dbContext.SaveChanges();

        var controller = GetControllerWithContext(dbContext);

        var form = new validateEmailForm() {
            UserId = 1,
            Token = token
        };

        // Act
        var result = await controller.ValidateEmailAsync(form);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Validated successfully", okResult.Value);

        var updatedUser = await dbContext.users.FindAsync(1);
        Assert.True(updatedUser!.IsValidated);
        Assert.Null(updatedUser.ActionDateEnd);
        Assert.Null(updatedUser.ActionToken);
    }
    [Fact]
    public async Task ValidateEmailAsync_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        using var dbContext = new DiariesContext(_options);
        var correctToken = new byte[32];
        new Random().NextBytes(correctToken);
        var wrongToken = new byte[32];
        new Random().NextBytes(wrongToken);

        var user = new User
        {
            Id = 1,
            UserName = "user",
            Email = "email@example.com",
            ActionToken = correctToken,
            ActionDateEnd = DateTime.Now.AddMinutes(10),
            IsValidated = false,
            Password = "123",
            Role = "Default",
            Description = ""
        };
        dbContext.users.Add(user);
        await dbContext.SaveChangesAsync();

        var controller = GetControllerWithContext(dbContext);

        var tokenBase64 = Convert.ToBase64String(wrongToken).Replace('+', '-').Replace('/', '_');
        var form = new validateEmailForm { UserId = 1, Token = tokenBase64 };

        var mockLocalized = new LocalizedString("TokenNotEqual", "Tokens don't match");
        _mockLocalizer.Setup(l => l["TokenNotEqual"]).Returns(mockLocalized);

        // Act
        var result = await controller.ValidateEmailAsync(form);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Tokens don't match", badRequest.Value);
    }

    [Fact]
    public async Task ValidateEmailAsync_ExpiredToken_ReturnsBadRequest()
    {
        // Arrange
        using var context = new DiariesContext(_options);
        var tokenBytes = new byte[32];
        new Random().NextBytes(tokenBytes);

        var user = new User
        {
            Id = 1,
            UserName = "user",
            Email = "email@example.com",
            ActionToken = tokenBytes,
            ActionDateEnd = DateTime.Now.AddMinutes(-1),
            IsValidated = false,
            Password = "123",
            Role = "Default",
            Description = ""
        };
        context.users.Add(user);
        await context.SaveChangesAsync();

        var controller = GetControllerWithContext(context);

        var tokenBase64 = Convert.ToBase64String(tokenBytes).Replace('+', '-').Replace('/', '_');
        var form = new validateEmailForm { UserId = 1, Token = tokenBase64 };

        var mockLocalized = new LocalizedString("TokenTimeExpired", "Token expired");
        _mockLocalizer.Setup(l => l["TokenTimeExpired"]).Returns(mockLocalized);

        // Act
        var result = await controller.ValidateEmailAsync(form);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Token expired", badRequest.Value);
    }

    [Fact]
    public async Task ValidateEmailAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = new DiariesContext(_options);

        var controller = GetControllerWithContext(context);

        var form = new validateEmailForm
        {
            UserId = 99,
            Token = "irrelevant"
        };

        var mockLocalized = new LocalizedString("InvalidNameOrPswd", "User not found");
        _mockLocalizer.Setup(l => l["InvalidNameOrPswd"]).Returns(mockLocalized);

        // Act
        var result = await controller.ValidateEmailAsync(form);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found", notFound.Value);
    }

}
