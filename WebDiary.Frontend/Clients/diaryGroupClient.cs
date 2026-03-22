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
        try
        {
            var response = await AuthorizedRequestAsync(() => httpClient.GetAsync("groups"));
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                return new List<DiaryGroup>();
            }
            return await response.Content.ReadFromJsonAsync<List<DiaryGroup>>() ?? new List<DiaryGroup>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetGroupsAsync error: {ex.Message}");
            return new List<DiaryGroup>();
        }
    }
        
    virtual public async Task<List<DiaryGroup>> GetGroupsOfUserAsync(int userId)
    {
        try
        {
            var response = await AuthorizedRequestAsync(() => httpClient.GetAsync($"groups/ofuser/{userId}"));
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                return new List<DiaryGroup>();
            }
            return await response.Content.ReadFromJsonAsync<List<DiaryGroup>>() ?? new List<DiaryGroup>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetGroupsOfUserAsync error: {ex.Message}");
            return new List<DiaryGroup>();
        }
    }
        
    virtual public async Task<List<DiaryGroup>> GetArchivedGroupsAsync(int userId)
    {
        try
        {
            var response = await AuthorizedRequestAsync(() => httpClient.GetAsync("groups/archived"));
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                return new List<DiaryGroup>();
            }
            return await response.Content.ReadFromJsonAsync<List<DiaryGroup>>() ?? new List<DiaryGroup>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetArchivedGroupsAsync error: {ex.Message}");
            return new List<DiaryGroup>();
        }
    }
        
    virtual public async Task<DiaryGroup> GetGroupAsync(int id)
    {
        try
        {
            var response = await AuthorizedRequestAsync(() => httpClient.GetAsync($"groups/{id}"));
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get group: {response.StatusCode} - {errorContent}");
            }
            var group = await response.Content.ReadFromJsonAsync<DiaryGroup>();
            return group ?? throw new Exception("Group wasn't found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetGroupAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task AddGroupAsync(DiaryGroup group) {
        try
        {
            var response = await AuthorizedRequestAsync(() =>
                httpClient.PostAsJsonAsync<DiaryGroup>("groups", group));
            if(!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to add group: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AddGroupAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateGroupAsync(DiaryGroup newGroup) {
        try
        {
            var response = await AuthorizedRequestAsync(() =>
                httpClient.PutAsJsonAsync<DiaryGroup>($"groups/{newGroup.Id}", newGroup));
            if(!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update group: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateGroupAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteGroupAsync(int id) {
        try
        {
            var response = await AuthorizedRequestAsync(() => httpClient.DeleteAsync($"groups/{id}"));
            if(!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to delete group: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteGroupAsync error: {ex.Message}");
            throw;
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
