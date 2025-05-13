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
        var mockUserClient = new Mock<UserClient>();
        mockUserClient.Setup(u => u.GetUsersAsync()).ReturnsAsync(new List<User> {
            new User { UserName = "existing" }
        });

        Services.AddSingleton<UserClient>(mockUserClient.Object);
        RegisterMocks(skipUserClient: true);

        var cut = RenderComponent<Signin>();
        cut.Instance.user = new User { UserName = "existing", Password = "123", Email = "test@example.com" };

        // Act
        await cut.InvokeAsync(() => cut.Instance.SignIn());

        // Assert
        Assert.Contains("UsernameAlreadyUsed", cut.Markup); // or use Localizer if returning string
    }

    [Fact]
    public async Task SigninPage_UniqueUser_SetsMessage()
    {
        // Arrange
        var mockUserClient = new Mock<UserClient>();
        mockUserClient.Setup(u => u.GetUsersAsync()).ReturnsAsync(new List<User>());
        mockUserClient.Setup(u => u.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(new User { Id = 123, UserName = "" });

        Services.AddSingleton<UserClient>(mockUserClient.Object);
        Services.AddSingleton<IStringLocalizer<UsersResource>>(new DummyLocalizer());

        var handler = new MockHttpMessageHandler((req) =>
        {
            if (req.RequestUri!.ToString().Contains("isunique"))
                return new HttpResponseMessage(HttpStatusCode.BadRequest); // Email is unique
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        Services.AddSingleton<HttpClient>(new HttpClient(handler));

        var cut = RenderComponent<Signin>();
        cut.Instance.user = new User { UserName = "newuser", Password = "pass", Email = "unique@example.com" };

        // Act
        await cut.InvokeAsync(() => cut.Instance.SignIn());

        // Assert
        Assert.Contains("ValidateMust", cut.Markup);
    }

    // --------------------
    // Helper registration
    // --------------------
    private void RegisterMocks(bool skipUserClient = false)
    {
        Services.AddSingleton<IStringLocalizer<UsersResource>>(new DummyLocalizer());
        Services.AddSingleton<HttpClient>(new HttpClient(new MockHttpMessageHandler()));
        if (!skipUserClient)
            Services.AddSingleton(Mock.Of<UserClient>());
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

    private class DummyLocalizer : IStringLocalizer<UsersResource>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments] => new(name, name);
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
        public IStringLocalizer WithCulture(System.Globalization.CultureInfo culture) => this;
    }
}
