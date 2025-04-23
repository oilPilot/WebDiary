using System;
using Microsoft.AspNetCore.Components.Authorization;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Models.Auth;

namespace WebDiary.Frontend.Clients;

public class DiaryClient(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<List<Diary>> GetDiariesAsync() {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.GetAsync($"diaries"));
        return await response.Content.ReadFromJsonAsync<List<Diary>>() ?? new List<Diary>();
    }
    public async Task<List<Diary>> GetDiariesOfGroupAsync(int groupId) {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.GetAsync($"diaries/ofgroup/{groupId}"));
        return await response.Content.ReadFromJsonAsync<List<Diary>>() ?? new List<Diary>();
    }
        
    public async Task<Diary> GetDiaryAsync(int id) {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.GetAsync($"diaries/{id}"));
        return await response.Content.ReadFromJsonAsync<Diary>() ?? throw new Exception("Diary wasn't found");
    }

    public async Task AddDiaryAsync(Diary diary) {
        var response = await ((CustomAuthenticationStateProvider)authenticationStateProvider).AuthorizedRequestAsync(() =>
            httpClient.PostAsJsonAsync<Diary>("diaries", diary));
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    public async Task UpdateDiaryAsync(Diary newDiary) {
        var response = await httpClient.PutAsJsonAsync<Diary>($"diaries/{newDiary.Id}", newDiary);
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    public async Task DeleteDiaryAsync(int id) {
        var response = await httpClient.DeleteAsync($"diaries/{id}");
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }
}
