using Microsoft.AspNetCore.Components.Authorization;
using Serilog;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Models.Auth;

namespace WebDiary.Frontend.Clients;

public class LogRecordClient(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
{
    virtual public async Task<List<LogRecord>> GetLogRecordsAsync(int count = 100) {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync($"logs/{count}") );
            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("GetLogRecordsAsync failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                return new List<LogRecord>();
            }
            return await response.Content.ReadFromJsonAsync<List<LogRecord>>() ?? new List<LogRecord>();
        } catch (Exception ex) {
            Log.Error(ex, "GetLogRecordsAsync error");
            return new List<LogRecord>();
        }
    }

    public async Task ClearLogsAsync() {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.DeleteAsync($"logs/") );
            if(!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("ClearLogsAsync failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                throw new Exception($"Failed to clear logs: {response.StatusCode}");
            }
        } catch (Exception ex) {
            Log.Error(ex, "ClearLogsAsync error");
            throw;
        }
    }
}
