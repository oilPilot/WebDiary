using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.AddDiariesEndpoints();
app.AddGroupsEndpoints();
app.AddUsersEndpoint();
app.MapControllers();

await app.MigrateDbAsync();

app.Run();
