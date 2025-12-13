using Microsoft.AspNetCore.Components.Authorization;
using Serilog;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Models.Auth;

namespace WebDiary.Frontend.Clients;

public class LogRecordClient(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
{
    virtual public async Task<List<LogRecord>> GetLogRecordsAsync(int count = 100) {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.GetAsync($"logs/{count}") );
        return await response.Content.ReadFromJsonAsync<List<LogRecord>>() ?? new List<LogRecord>();
    }

    public async Task ClearLogsAsync() {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.DeleteAsync($"logs/") );
        if(!response.IsSuccessStatusCode) {
            Log.Error("Upon Clearing logs failed status code returned: " + response.StatusCode);
            throw new Exception();
        }
    }
}
