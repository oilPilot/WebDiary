using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using WebDiary.Frontend.Clients;
using Microsoft.Extensions.Localization;
using Microsoft;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Components.Pages;
using WebDiary.Frontend.Resources;

namespace WebDiary.Tests.Frontend;

public class MainPageTests : TestContext
{
    public MainPageTests()
    {
        // Register default services
        Services.AddSingleton<IStringLocalizer<MainResource>>(new DummyLocalizer());
        Services.AddSingleton<DiaryClient>(Mock.Of<DiaryClient>());
        Services.AddSingleton<DiaryGroupClient>(Mock.Of<DiaryGroupClient>());
        Services.AddSingleton<UserClient>(Mock.Of<UserClient>());
    }

    [Fact]
    public void MainPage_ShowsLoading_WhenDiariesAreNull()
    {
        // Arrange: mock diary list to be null and user is validated
        var diaryClient = new Mock<DiaryClient>();
        var diaryGroupClient = new Mock<DiaryGroupClient>();
        var userClient = new Mock<UserClient>();

        var group = new DiaryGroup { Id = 5, Name = "Group A" };
        diaryGroupClient.Setup(c => c.GetGroupAsync(5)).ReturnsAsync(group);
        diaryClient.Setup(c => c.GetDiariesOfGroupAsync(5)).ReturnsAsync((List<Diary>?)null);

        var fakeUser = new User { Id = 1, IsValidated = true, UserName = "123" };
        userClient.Setup(c => c.GetUserByIdAsync(1)).ReturnsAsync(fakeUser);

        Services.AddSingleton(diaryClient.Object);
        Services.AddSingleton(diaryGroupClient.Object);
        Services.AddSingleton(userClient.Object);
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthProvider(authenticated: true, userId: 1));

        // Act
        var cut = RenderComponent<MainPage>(parameters => parameters.Add(p => p.id, 5));

        // Assert
        cut.MarkupMatches(@"<p>LoadingDiaries</p>");
    }

    [Fact]
    public void MainPage_ShowsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthProvider(authenticated: false));

        var cut = RenderComponent<MainPage>(parameters => parameters.Add(p => p.id, 1));

        // Assert
        Assert.Contains("UnathorizedTitle", cut.Markup);
        Assert.Contains("UnathorizedText", cut.Markup);
    }

    [Fact]
    public void MainPage_ShowsGroupedDiaries_WhenValidated()
    {
        // Arrange
        var diaryClient = new Mock<DiaryClient>();
        var diaryGroupClient = new Mock<DiaryGroupClient>();
        var userClient = new Mock<UserClient>();

        var group = new DiaryGroup { Id = 7, Name = "Group B" };
        var date = DateOnly.FromDateTime(DateTime.Now);
        var time = TimeOnly.FromDateTime(DateTime.Now);
        var diaries = new List<Diary>
        {
            new Diary { Id = 1, Date = date, Time = time, Text = "First", GroupId = 7 },
            new Diary { Id = 2, Date = date, Time = time.AddHours(1), Text = "Second", GroupId = 7 }
        };

        diaryGroupClient.Setup(c => c.GetGroupAsync(7)).ReturnsAsync(group);
        diaryClient.Setup(c => c.GetDiariesOfGroupAsync(7)).ReturnsAsync(diaries);
        userClient.Setup(c => c.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, IsValidated = true, UserName = "123" });

        Services.AddSingleton(diaryClient.Object);
        Services.AddSingleton(diaryGroupClient.Object);
        Services.AddSingleton(userClient.Object);
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthProvider(authenticated: true, userId: 1));

        // Act
        var cut = RenderComponent<MainPage>(parameters => parameters.Add(p => p.id, 7));

        // Assert
        Assert.Contains("First", cut.Markup);
        Assert.Contains("Second", cut.Markup);
        Assert.Contains("Group B", cut.Markup); // title from group
    }

    // -------------------
    // Helper mocks
    // -------------------

    private class TestAuthProvider : AuthenticationStateProvider
    {
        private readonly bool isAuthenticated;
        private readonly int userId;

        public TestAuthProvider(bool authenticated = true, int userId = 1)
        {
            this.isAuthenticated = authenticated;
            this.userId = userId;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = isAuthenticated
                ? new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, "Tester"),
                    new Claim("userId", userId.ToString())
                }, "testAuth")
                : new ClaimsIdentity();

            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }
    }

    private class DummyLocalizer : IStringLocalizer<MainResource>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments] => new(name, name);
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
        public IStringLocalizer WithCulture(System.Globalization.CultureInfo culture) => this;
    }
}
