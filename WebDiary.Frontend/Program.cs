using WebDiary.Frontend.Clients;
using WebDiary.Frontend.Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebDiary.Frontend.Models.Auth;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiConnection"] ?? throw new Exception ("Api wasn't in ApiConnection."))});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DiaryGroupClient>();
builder.Services.AddScoped<DiaryClient>();
builder.Services.AddScoped<UserClient>();
builder.Services.AddBlazoredLocalStorage();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/* TODO:
    Major tasks:
        Make managing account;
    Med tasks:
        Make diaries pinned to users;
        Make more beatiful design for Users pages;
    Little tasks:
        Make "JWT:key" from config a User Secret;

*/
