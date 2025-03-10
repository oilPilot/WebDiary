using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebDiary.Data;
using WebDiary.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connstring = builder.Configuration.GetConnectionString("DiariesConnection");
builder.Services.AddDbContext<DiariesContext>(options => {
    options.UseSqlServer(connstring);
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.AddDiariesEndpoints();
app.AddGroupsEndpoints();
app.AddUsersEndpoint();

await app.MigrateDbAsync();

app.Run();
