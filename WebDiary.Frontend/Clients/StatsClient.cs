using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Models.Auth;

namespace WebDiary.Frontend.Clients;

public class StatsClient(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<HttpResponseMessage> GetStatisticsAsync(int loggedUserId,
        DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        return await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.GetAsync($"stats/fordates?userId={loggedUserId}&" +
                $"startDate={startDate?.ToString("yyyy-MM-dd")}&endDate={endDate?.ToString("yyyy-MM-dd")}"));
    }

    public async Task<int> GetNewUsersFromDate(DateOnly? startDate) {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync($"stats/adminNewUsers?fromPeriod={startDate?.ToString("yyyy-MM-dd")}"));
            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                return 0;
            }
            return await response.Content.ReadFromJsonAsync<int>();
        } catch (Exception ex) {
            return 0;
        }
    }

    public async Task<int> GetNewEntriesFromDate(DateOnly? startDate) {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync($"stats/adminNewEntries?fromPeriod={startDate?.ToString("yyyy-MM-dd")}"));
            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                return 0;
            }
            return await response.Content.ReadFromJsonAsync<int>();
        } catch (Exception ex) {
            return 0;
        }
    }

    public async Task<int> GetActiveUsersFromDate(DateOnly? startDate) {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync($"stats/adminActiveUsers?fromPeriod={startDate?.ToString("yyyy-MM-dd")}"));
            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                return 0;
            }
            return await response.Content.ReadFromJsonAsync<int>();
        } catch (Exception ex) {
            return 0;
        }
    }

    public async Task<List<User>> GetInactiveUsers() {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync($"stats/adminInactiveUsers"));
            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new List<User>();
            }
            return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
        } catch (Exception ex) {
            return new List<User>();
        }
    }

    public async Task<List<AdminStatsModel>> GetLast30DaysStats() {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync($"stats/admin30DayStatistics"));
            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new List<AdminStatsModel>();
            }
            return await response.Content.ReadFromJsonAsync<List<AdminStatsModel>>()
                ?? new List<AdminStatsModel>();
        } catch (Exception ex) {
            return new List<AdminStatsModel>();
        }
    }

    public async Task<List<UserActivity>> GetMostActiveUsers(int limit = 5) {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync($"stats/adminMostActiveUsers?limit={limit}"));
            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new List<UserActivity>();
            }
            return await response.Content.ReadFromJsonAsync<List<UserActivity>>() ?? new List<UserActivity>();
        } catch (Exception ex) {
            return new List<UserActivity>();
        }
    }
}
