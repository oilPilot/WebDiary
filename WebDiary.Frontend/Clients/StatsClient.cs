using Microsoft.AspNetCore.Components.Authorization;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Models.Auth;

namespace WebDiary.Frontend.Clients;

public class StatsClient(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<HttpResponseMessage> GetStatisticsAsync(int loggedUserId,
        DateTimeOffset? startDate, DateTimeOffset? endDate) =>
        await httpClient.GetAsync($"stats/fordates?userId={loggedUserId}&" +
            $"startDate={startDate?.ToString("yyyy-MM-dd")}&endDate={endDate?.ToString("yyyy-MM-dd")}");

    public async Task<int> GetNewUsersFromDate(DateOnly? startDate) {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.GetAsync($"stats/adminNewUsers?fromPeriod{startDate?.ToString("yyyy-MM-dd")}"));
        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task<int> GetNewEntriesFromDate(DateOnly? startDate) {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.GetAsync($"stats/adminNewEntries?fromPeriod{startDate?.ToString("yyyy-MM-dd")}"));
        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task<int> GetActiveUsersFromDate(DateOnly? startDate) {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.GetAsync($"stats/adminActiveUsers?fromPeriod{startDate?.ToString("yyyy-MM-dd")}"));
        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task<List<User>> GetInactiveUsers() {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.GetAsync($"stats/adminInactiveUsers"));
        return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
    }

    public async Task<List<(DateOnly dateOfData, int entriesCount, int symbolsCount)>> GetLast30DaysStats() {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.GetAsync($"stats/admin30DayStatistics"));
        return await response.Content.ReadFromJsonAsync<List<(DateOnly dateOfData, int entriesCount, int symbolsCount)>>()
            ?? new List<(DateOnly dateOfData, int entriesCount, int symbolsCount)>();
    }
}
