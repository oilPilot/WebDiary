using System;
using Microsoft.AspNetCore.Components.Authorization;
using Serilog;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Models.Auth;

namespace WebDiary.Frontend.Clients;

public class SearchClient(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<List<Diary>> SearchEntriesAsync(SearchFilter filter)
    {
        try
        {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.PostAsJsonAsync("search/entries", filter));
            
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Search request failed with status: {StatusCode}", response.StatusCode);
                return new List<Diary>();
            }

            return await response.Content.ReadFromJsonAsync<List<Diary>>() ?? new List<Diary>();
        }
        catch (Exception ex)
        {
            Log.Error("Error searching entries: {@Exception}", ex);
            return new List<Diary>();
        }
    }

    public async Task<List<string>> GetMoodsAsync()
    {
        try
        {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync("search/moods"));

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Get moods request failed with status: {StatusCode}", response.StatusCode);
                return new List<string>();
            }

            return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
        }
        catch (Exception ex)
        {
            Log.Error("Error retrieving moods: {@Exception}", ex);
            return new List<string>();
        }
    }

    public async Task<List<string>> GetTagsAsync()
    {
        try
        {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync("search/tags"));

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Get tags request failed with status: {StatusCode}", response.StatusCode);
                return new List<string>();
            }

            return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
        }
        catch (Exception ex)
        {
            Log.Error("Error retrieving tags: {@Exception}", ex);
            return new List<string>();
        }
    }
}

public class SearchFilter
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Mood { get; set; }
    public List<string>? Tags { get; set; }
    public string? Content { get; set; }
}
