using System;
using Microsoft.AspNetCore.Components.Authorization;
using Serilog;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Models.Auth;

namespace WebDiary.Frontend.Clients;

public class DiaryClient(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
{
    virtual public async Task<List<Diary>> GetDiariesAsync() {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync($"diaries"));
            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("GetDiariesAsync failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                return new List<Diary>();
            }
            return await response.Content.ReadFromJsonAsync<List<Diary>>() ?? new List<Diary>();
        } catch (Exception ex) {
            Log.Error(ex, "GetDiariesAsync error");
            return new List<Diary>();
        }
    }
    virtual public async Task<List<Diary>> GetDiariesOfGroupAsync(int groupId) {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync($"diaries/ofgroup/{groupId}"));
            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("GetDiariesOfGroupAsync failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                return new List<Diary>();
            }
            return await response.Content.ReadFromJsonAsync<List<Diary>>() ?? new List<Diary>();
        } catch (Exception ex) {
            Log.Error(ex, "GetDiariesOfGroupAsync error");
            return new List<Diary>();
        }
    }
        
    virtual public async Task<Diary> GetDiaryAsync(int id) {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.GetAsync($"diaries/{id}"));
            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get diary: {response.StatusCode} - {errorContent}");
            }
            var diary = await response.Content.ReadFromJsonAsync<Diary>();
            return diary ?? throw new Exception("Diary wasn't found");
        } catch (Exception ex) {
            Log.Error(ex, "GetDiaryAsync error");
            throw;
        }
    }

    public async Task<Diary?> AddDiaryAsync(Diary diary) {
        try {
            var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
                httpClient.PostAsJsonAsync<Diary>("diaries", diary));
            if(!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("Adding diary was unsuccessful: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                throw new Exception($"Failed to add diary: {response.StatusCode}");
            }
            return await response.Content.ReadFromJsonAsync<Diary>();
        } catch (Exception ex) {
            Log.Error(ex, "AddDiaryAsync error");
            throw;
        }
    }

    public async Task UpdateDiaryAsync(Diary newDiary) {
        try {
            var response = await httpClient.PutAsJsonAsync<Diary>($"diaries/{newDiary.Id}", newDiary);
            if(!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("Updating diary was unsuccessful: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                throw new Exception($"Failed to update diary: {response.StatusCode}");
            }
        } catch (Exception ex) {
            Log.Error(ex, "UpdateDiaryAsync error");
            throw;
        }
    }

    public async Task DeleteDiaryAsync(int id) {
        try {
            var response = await httpClient.DeleteAsync($"diaries/{id}");
            if(!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("Deleting diary was unsuccessful: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                throw new Exception($"Failed to delete diary: {response.StatusCode}");
            }
        } catch (Exception ex) {
            Log.Error(ex, "DeleteDiaryAsync error");
            throw;
        }
    }
}
