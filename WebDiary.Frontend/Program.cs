using WebDiary.Frontend.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebDiary.Frontend.Models.Auth;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using WebDiary.Frontend.Components;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiConnection"] ?? throw new Exception ("Api wasn't in ApiConnection."))});
builder.Services.AddScoped<DiaryGroupClient>();
builder.Services.AddScoped<DiaryClient>();
builder.Services.AddScoped<UserClient>();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddLocalization();
builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseAntiforgery();

// add localization. If changed should also be changed in Backend (error messages are working with resources).
var supportedCultures = new[] { "en", "de"};
var localizationOptions = new RequestLocalizationOptions().
    SetDefaultCulture(supportedCultures[0]).AddSupportedCultures(supportedCultures).AddSupportedUICultures(supportedCultures);
app.MapControllers();
app.UseRequestLocalization(localizationOptions);

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/* TODO:
    Major tasks:
        Make everything look better;
    Med tasks:
        Nothing;
    Little tasks:
        Difference beetween http and https and what they are;

*/
