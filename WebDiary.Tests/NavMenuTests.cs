using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Components.Authorization;
using WebDiary.Frontend.Clients;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using WebDiary.Frontend.Resources;
using WebDiary.Frontend.Components.Layout;
using WebDiary.Frontend.Models;

namespace WebDiary.Tests.Frontend;

public class NavMenuTests : TestContext
{
    public NavMenuTests()
    {
        // Shared services across all tests
        Services.AddSingleton<IStringLocalizer<LayoutResource>>(new DummyLocalizer());
        Services.AddSingleton<UserClient>(Mock.Of<UserClient>());
        Services.AddSingleton<DiaryGroupClient>(Mock.Of<DiaryGroupClient>());
    }

    [Fact]
    public void NavMenu_ShowsGuestOptions_WhenNotAuthenticated()
    {
        // Arrange
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthProvider(false));

        // Act
        var cut = RenderComponent<NavMenu>();

        // Assert
        Assert.Contains("SigninTitle", cut.Markup);
        Assert.Contains("LoginTitle", cut.Markup);
    }

    [Fact]
    public void NavMenu_ShowsAuthenticatedOptions_WhenAuthenticated()
    {
        // Arrange
        var diaryGroupClient = new Mock<DiaryGroupClient>();
        diaryGroupClient.Setup(c => c.GetGroupsOfUserAsync(It.IsAny<int>())).ReturnsAsync(new List<DiaryGroup>());
        Services.AddSingleton<DiaryGroupClient>(diaryGroupClient.Object);

        var userClient = new Mock<UserClient>();
        userClient.Setup(c => c.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(new User() { UserName = "123" } );
        Services.AddSingleton<UserClient>(userClient.Object);

        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthProvider(true, 42));

        // Act
        var cut = RenderComponent<NavMenu>();

        // Assert
        Assert.Contains("ManageTitle", cut.Markup);
        Assert.Contains("LogoutTitle", cut.Markup);
    }

    [Fact]
    public void NavMenu_ShowsGroupNames_WhenGroupsExist()
    {
        // Arrange
        var groups = new List<DiaryGroup> {
            new DiaryGroup { Id = 1, Name = "Private" },
            new DiaryGroup { Id = 2, Name = "WorkStuff" }
        };

        var diaryGroupClient = new Mock<DiaryGroupClient>();
        diaryGroupClient.Setup(c => c.GetGroupsOfUserAsync(It.IsAny<int>())).ReturnsAsync(groups);
        Services.AddSingleton<DiaryGroupClient>(diaryGroupClient.Object);

        var userClient = new Mock<UserClient>();
        userClient.Setup(c => c.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(new User() { UserName = "123" } );
        Services.AddSingleton<UserClient>(userClient.Object);

        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthProvider(true, 7));

        // Act
        var cut = RenderComponent<NavMenu>();

        // Assert
        Assert.Contains("Private", cut.Markup);
        Assert.Contains("WorkStuff", cut.Markup);
    }

    // ------------------
    // Helpers
    // ------------------

    private class DummyLocalizer : IStringLocalizer<LayoutResource>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments] => new(name, name);
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
        public IStringLocalizer WithCulture(System.Globalization.CultureInfo culture) => this;
    }

    private class TestAuthProvider : AuthenticationStateProvider
    {
        private readonly bool isAuthenticated;
        private readonly int userId;

        public TestAuthProvider(bool authenticated, int userId = 1)
        {
            this.isAuthenticated = authenticated;
            this.userId = userId;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = isAuthenticated
                ? new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim("userId", userId.ToString())
                }, "test")
                : new ClaimsIdentity();

            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }
    }
}
