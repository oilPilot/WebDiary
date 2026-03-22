using WebDiary.Frontend.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebDiary.Frontend.Models.Auth;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using Blazored.SessionStorage;
using Serilog;
using WebDiary.Frontend;
using WebDiary.Frontend.Components;

var builder = WebApplication.CreateBuilder(args);
var connstring = builder.Configuration.GetConnectionString("DiariesConnection");

using var log = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(connstring, "logs")
    .CreateLogger();
Log.Logger = log;
Log.Information("Global logger has been configured");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiConnection"] ?? throw new Exception ("Api wasn't in ApiConnection."))});
AddClients();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

builder.Services.AddLocalization();
builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

Log.Information("Added services to container");

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
    SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.MapControllers();
app.UseRequestLocalization(localizationOptions);

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Log.Information("App is ready to run");

try
{
    if (app.Environment.IsDevelopment())
        Log.Information("In Development environment");
    app.Run();
} catch (Exception Ex)
{
    Log.Fatal("Catched exception upon opening app: {Exception}", Ex);
}

void AddClients()
{
    builder.Services.AddScoped<DiaryGroupClient>();
    builder.Services.AddScoped<DiaryClient>();
    builder.Services.AddScoped<SearchClient>();
    builder.Services.AddScoped<UserClient>();
    builder.Services.AddScoped<LogRecordClient>();
    builder.Services.AddScoped<StatsClient>();
}
