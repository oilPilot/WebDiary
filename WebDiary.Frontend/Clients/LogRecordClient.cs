using Serilog;
using WebDiary.Frontend.Models;

namespace WebDiary.Frontend.Clients;

public class LogRecordClient(HttpClient httpClient)
{
    virtual public async Task<List<LogRecord>> GetLogRecordsAsync(int count = 100) =>
        await httpClient.GetFromJsonAsync<List<LogRecord>>($"logs/{count}") ?? new List<LogRecord>();

    public async Task ClearLogsAsync() {
        var response = await httpClient.DeleteAsync($"logs/");
        if(!response.IsSuccessStatusCode) {
            Log.Error("Upon Clearing logs failed status code returned: " + response.StatusCode);
            throw new Exception();
        }
    }
}
