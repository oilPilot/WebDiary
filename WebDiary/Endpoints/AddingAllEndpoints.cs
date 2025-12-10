
namespace WebDiary.Endpoints;

public static class AddEndpoints
{
    public static void AddEveryEndpoint(this WebApplication app)
    {
        app.AddDiariesEndpoints();
        app.AddGroupsEndpoints();
        app.AddUsersEndpoint();
        app.AddLogsEndpoint();
    }
}