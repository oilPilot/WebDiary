using System;
using Microsoft.AspNetCore.Components.Authorization;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Models.Auth;

namespace WebDiary.Frontend.Clients;

public class DiaryGroupClient
{
    private readonly HttpClient httpClient;
    private readonly CustomAuthenticationStateProvider? authStateProvider;

    public DiaryGroupClient(HttpClient httpClient, AuthenticationStateProvider? authenticationStateProvider = null)
    {
        this.httpClient = httpClient;
        authStateProvider = authenticationStateProvider as CustomAuthenticationStateProvider;
    }

    virtual public async Task<List<DiaryGroup>> GetGroupsAsync()
    {
        var response = await AuthorizedRequestAsync(() => httpClient.GetAsync("groups"));
        return await response.Content.ReadFromJsonAsync<List<DiaryGroup>>() ?? new List<DiaryGroup>();
    }
        
    virtual public async Task<List<DiaryGroup>> GetGroupsOfUserAsync(int userId)
    {
        var response = await AuthorizedRequestAsync(() => httpClient.GetAsync($"groups/ofuser/{userId}"));
        return await response.Content.ReadFromJsonAsync<List<DiaryGroup>>() ?? new List<DiaryGroup>();
    }
        
    virtual public async Task<DiaryGroup> GetGroupAsync(int id)
    {
        var response = await AuthorizedRequestAsync(() => httpClient.GetAsync($"groups/{id}"));
        return await response.Content.ReadFromJsonAsync<DiaryGroup>() ?? throw new Exception("Group wasn't found");
    }

    public async Task AddGroupAsync(DiaryGroup group) {
        var response = await AuthorizedRequestAsync(() =>
            httpClient.PostAsJsonAsync<DiaryGroup>("groups", group));
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    public async Task UpdateGroupAsync(DiaryGroup newGroup) {
        var response = await AuthorizedRequestAsync(() =>
            httpClient.PutAsJsonAsync<DiaryGroup>($"groups/{newGroup.Id}", newGroup));
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    public async Task DeleteGroupAsync(int id) {
        var response = await AuthorizedRequestAsync(() => httpClient.DeleteAsync($"groups/{id}"));
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    private async Task<HttpResponseMessage> AuthorizedRequestAsync(Func<Task<HttpResponseMessage>> action)
    {
        if (authStateProvider == null)
        {
            return await action();
        }

        return await authStateProvider.AuthorizedRequestAsync(action);
    }
}
