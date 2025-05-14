using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WebDiary.Data;
using WebDiary.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connstring = builder.Configuration.GetConnectionString("DiariesConnection");
builder.Services.AddDbContext<DiariesContext>(options => {
    options.UseSqlServer(connstring);
});
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

var app = builder.Build();

using var log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Log.Logger = log;
Log.Information("Global logger has been configured");

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

try {
if(app.Environment.IsDevelopment()) {
    await app.MigrateDbAsync();
    Log.Information("Migrated DB");
}

app.Run();
} catch(Exception Ex) {
    Log.Fatal("Catched exception upon opening app: {Exception}", Ex);
}
