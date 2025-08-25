using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using WebDiary.Data;
using WebDiary.Endpoints;

var builder = WebApplication.CreateBuilder(args);

using var log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Log.Logger = log;
Log.Information("Global logger has been configured");

var connstring = builder.Configuration.GetConnectionString("DiariesConnection");
builder.Services.AddDbContext<DiariesContext>(options =>
{
    options.UseSqlServer(connstring);
});
Log.Information("Configured DbContext with connection string {ConnectionString}", connstring);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters() {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateIssuer = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuerSigningKey = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateAudience = true
        });
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddLocalization();
builder.Services.AddEndpointsApiExplorer();
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

app.UseAuthentication();
app.UseAuthorization();
Log.Information("Added Authentication and Authorization to app");

var supportedCultures = new[] { "en", "de"};
var localizationOptions = new RequestLocalizationOptions().
    SetDefaultCulture(supportedCultures[0]).AddSupportedCultures(supportedCultures).AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);
Log.Information("Added Localization to app");

app.MapGet("/", () => "Hello World!");

app.AddDiariesEndpoints();
app.AddGroupsEndpoints();
app.AddUsersEndpoint();
app.MapControllers();
Log.Information("Added Endpoints and Controllers to app");

try
{

    if (app.Environment.IsDevelopment())
    {
        await app.MigrateDbAsync();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyDiaryV1");
        });
        Log.Information("Migrated DB");
    }

    app.Run();
}
catch (Exception Ex)
{
    Log.Fatal("Catched exception upon opening app: {Exception}", Ex);
}
