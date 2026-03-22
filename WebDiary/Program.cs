using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using WebDiary.Data;
using WebDiary.Endpoints;
using WebDiary.Hubs;

var builder = WebApplication.CreateBuilder(args);
var connstring = builder.Configuration.GetConnectionString("DiariesConnection");

using var log = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(connstring, "logs")
    .CreateLogger();
Log.Logger = log;
Log.Information("Global logger has been configured");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddDbContextPool<DiariesContext>(options =>
{
    options.UseNpgsql(connstring);
});
Log.Information("Configured DbContext with connection string" /*{ConnectionString}", connstring 'ONLY FOR DEVELOPERS'*/);

const string ExternalCookieScheme = "External";
const string GoogleScheme = "Google";
const string GitHubScheme = "GitHub";

var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateIssuer = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuerSigningKey = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateAudience = true
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    })
    .AddCookie(ExternalCookieScheme, options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = false;
    });

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddOAuth(GoogleScheme, options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.SignInScheme = ExternalCookieScheme;
        options.CallbackPath = "/signin-google";
        options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        options.TokenEndpoint = "https://oauth2.googleapis.com/token";
        options.UserInformationEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.SaveTokens = true;
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                using var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();

                using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
                context.RunClaimActions(payload.RootElement);
            },
            OnRemoteFailure = context =>
            {
                var failureRedirect = BuildOAuthFailureRedirect(context.Properties, builder.Configuration);
                context.Response.Redirect(failureRedirect);
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });
}
else
{
    Log.Warning("Google OAuth is not configured. Set Authentication:Google:ClientId and Authentication:Google:ClientSecret to enable it.");
}

var gitHubClientId = builder.Configuration["Authentication:GitHub:ClientId"];
var gitHubClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
if (!string.IsNullOrWhiteSpace(gitHubClientId) && !string.IsNullOrWhiteSpace(gitHubClientSecret))
{
    authBuilder.AddOAuth(GitHubScheme, options =>
    {
        options.ClientId = gitHubClientId;
        options.ClientSecret = gitHubClientSecret;
        options.SignInScheme = ExternalCookieScheme;
        options.CallbackPath = "/signin-github";
        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
        options.UserInformationEndpoint = "https://api.github.com/user";
        options.Scope.Add("read:user");
        options.Scope.Add("user:email");
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.SaveTokens = true;
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("WebDiary", "1.0"));

                using var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();

                using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
                context.RunClaimActions(payload.RootElement);
            },
            OnRemoteFailure = context =>
            {
                var failureRedirect = BuildOAuthFailureRedirect(context.Properties, builder.Configuration);
                context.Response.Redirect(failureRedirect);
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });
}
else
{
    Log.Warning("GitHub OAuth is not configured. Set Authentication:GitHub:ClientId and Authentication:GitHub:ClientSecret to enable it.");
}
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddLocalization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<ISearchService, SearchService>();

Log.Information("Added Authentication, Authorization, Controllers and Localization to services");
/*
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo()
    {
        Version = "v1",
        Title = "My Diary API",
        Description = "Simple ASP.NET Core Web API for managing personal diary entries.",
        Contact = new OpenApiContact
        {
            Name = "Github url to my account",
            Url = new Uri("https://github.com/oilPilot")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});
*/

builder.Services.AddCors(options => {
    options.AddPolicy("MyPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["FrontendUrl"] ?? throw new Exception("FrontendUrl wasn't in configuration."))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("MyPolicy");

app.UseAuthentication();
app.UseAuthorization();
Log.Information("Added Authentication and Authorization to app");

var supportedCultures = new[] { "en", "de", "fr", "es" };
var localizationOptions = new RequestLocalizationOptions().
    SetDefaultCulture(supportedCultures[0]).AddSupportedCultures(supportedCultures).AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);
Log.Information("Added Localization to app");

app.MapGet("/health", () => "Healthy!").AllowAnonymous();

//app.AddDiariesEndpoints();
//app.AddGroupsEndpoints();
//app.AddUsersEndpoint();
//app.AddLogsEndpoint();
app.AddEveryEndpoint();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
Log.Information("Added Endpoints and Controllers to app");

try
{

    if (app.Environment.IsDevelopment())
    {
        await app.MigrateDbAsync();
        /* // Swagger don't work with supabase API, instead use supabase c# client library (possibly in future)
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyDiaryV1");
        });*/
        Log.Information("In Development environment");
        Log.Information("Migrated DB");
    }

    app.Run();
}
catch (Exception Ex)
{
    Log.Fatal("Catched exception upon opening app: {Exception}", Ex);
}

static string BuildOAuthFailureRedirect(AuthenticationProperties? properties, IConfiguration configuration)
{
    var returnUrl = GetOAuthReturnUrl(properties, configuration);
    return $"/auth/oauth/failure?returnUrl={Uri.EscapeDataString(returnUrl)}";
}

static string GetOAuthReturnUrl(AuthenticationProperties? properties, IConfiguration configuration)
{
    if (properties?.Items != null && properties.Items.TryGetValue("returnUrl", out var stored) && !string.IsNullOrWhiteSpace(stored))
    {
        if (Uri.TryCreate(stored, UriKind.Absolute, out _))
        {
            return stored;
        }
    }

    return configuration["FrontendUrl"] ?? "/";
}
