using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using WebDiary.Frontend.Components.Pages.Users;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebDiary.Frontend.Resources;

namespace WebDiary.Tests.Frontend;

public class LoginPageTests : TestContext {
    [Fact]
    public void LoginPage_Renders_LoginForm()
    {
        // Arrange: Register mocked services
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider());
        Services.AddSingleton<HttpClient>(new HttpClient(new MockHttpMessageHandler())); // dummy
        Services.AddSingleton<IStringLocalizer<UsersResource>>(Mock.Of<IStringLocalizer<UsersResource>>());

        // Act
        var cut = RenderComponent<Login>();

        // Assert: Check input fields and button
        cut.Find("input[type=text]");    // Username input
        cut.Find("input[type=password]"); // Password input
        cut.Find("button[type=submit]");  // Submit button
    }

    [Fact]
    public void LoginPage_ShowsValidationMessages_WhenSubmittedWithEmptyFields()
    {
        // Arrange
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider());
        Services.AddSingleton<HttpClient>(new HttpClient(new MockHttpMessageHandler()));
        Services.AddSingleton<IStringLocalizer<UsersResource>>(Mock.Of<IStringLocalizer<UsersResource>>());

        var cut = RenderComponent<Login>();

        // Act: Submit form without filling inputs
        cut.Find("form").Submit();

        // Assert: Should render validation summary
        Assert.Contains("validation-message", cut.Markup);
    }

    private class TestAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new System.Security.Claims.ClaimsIdentity();
            return Task.FromResult(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(identity)));
        }
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Always return 400 for this test
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
        }
    }
}