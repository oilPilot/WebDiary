using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using System.Net;
using WebDiary.Frontend.Clients;
using WebDiary.Frontend.Components.Pages.Users;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Resources;

namespace WebDiary.Tests.Frontend;

public class SigninTests : TestContext
{
    [Fact]
    public void SigninPage_Renders_Form()
    {
        // Arrange
        RegisterMocks();

        // Act
        var cut = RenderComponent<Signin>();

        // Assert
        cut.Find("input[type=text]"); // Username
        cut.Find("input[type=password]"); // Password
        cut.Find("button[type=submit]");
    }

    [Fact]
    public void SigninPage_EmptyFields_ShowsValidationErrors()
    {
        // Arrange
        RegisterMocks();
        var cut = RenderComponent<Signin>();

        // Act
        cut.Find("form").Submit();

        // Assert
        Assert.Contains("validation-message", cut.Markup);
    }

    [Fact]
    public async Task SigninPage_ExistingUsername_ShowsError()
    {
        // Arrange
        var mockUserClient = new Mock<UserClient>(new HttpClient(new MockHttpMessageHandler()));
        mockUserClient.Setup(u => u.GetUsersAsync()).ReturnsAsync(new List<User> {
            new User { UserName = "existing" }
        });

        Services.AddSingleton<UserClient>(mockUserClient.Object);
        RegisterMocks(skipUserClient: true);

        var cut = RenderComponent<Signin>();
        cut.Instance.user = new User { UserName = "existing", Password = "123", Email = "test@example.com" };

        // Act
        await cut.InvokeAsync(() => cut.Instance.SignIn());
        cut.Render();

        // Assert
        Assert.Contains("Your username is already used by another user. Please try another username.", cut.Markup);
    }

    [Fact]
    public async Task SigninPage_UniqueUser_SetsMessage()
    {
        // Arrange
        var handler = new MockHttpMessageHandler((req) =>
        {
            if (req.RequestUri!.ToString().Contains("isunique"))
                return new HttpResponseMessage(HttpStatusCode.BadRequest); // Email is unique
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5281") };
        Services.AddSingleton<HttpClient>(httpClient);
        var mockUserClient = new Mock<UserClient>(httpClient);
        mockUserClient.Setup(u => u.GetUsersAsync()).ReturnsAsync(new List<User>());
        mockUserClient.Setup(u => u.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(new User { Id = 123, UserName = "" });

        Services.AddSingleton<UserClient>(mockUserClient.Object);
        RegisterMocks(skipUserClient: true, skipHttpClient: true);

        var cut = RenderComponent<Signin>();
        cut.Instance.user = new User { UserName = "newuser", Password = "pass", Email = "unique@example.com" };

        // Act
        await cut.InvokeAsync(() => cut.Instance.SignIn());
        cut.Render();

        // Assert
        Assert.Contains("To use account you first must validate it in email. For that you have 5 hours. Message will soon come to your email.", cut.Markup);
    }

    // --------------------
    // Helper registration
    // --------------------
    private void RegisterMocks(bool skipUserClient = false, bool skipHttpClient = false)
    {
        Mock<IStringLocalizer<UsersResource>>? _mockLocalizer = new Mock<IStringLocalizer<UsersResource>>();
        _mockLocalizer.Setup(l => l["UsernameAlreadyUsed"]).Returns(new LocalizedString
            ("UsernameAlreadyUsed", "Your username is already used by another user. Please try another username."));
        _mockLocalizer.Setup(l => l["ValidateMust"]).Returns(new LocalizedString
            ("ValidateMust", "To use account you first must validate it in email. For that you have 5 hours. Message will soon come to your email."));
        Services.AddSingleton<IStringLocalizer<UsersResource>>(_mockLocalizer.Object);
        if (!skipHttpClient)
            Services.AddSingleton<HttpClient>(new HttpClient(new MockHttpMessageHandler()));
        if (!skipUserClient)
            Services.AddSingleton<UserClient>(new UserClient( new HttpClient(new MockHttpMessageHandler()) ));
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage>? handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage>? handler = null)
        {
            this.handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(handler?.Invoke(request) ?? new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
