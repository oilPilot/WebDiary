using System;
using Microsoft.AspNetCore.Components.Authorization;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Models.Auth;

namespace WebDiary.Frontend.Clients;

public class UserClient
{
    private readonly HttpClient httpClient;
    private readonly CustomAuthenticationStateProvider? authStateProvider;

    public UserClient(HttpClient httpClient, AuthenticationStateProvider? authenticationStateProvider = null)
    {
        this.httpClient = httpClient;
        authStateProvider = authenticationStateProvider as CustomAuthenticationStateProvider;
    }

    virtual public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            var response = await AuthorizedRequestAsync(() => httpClient.GetAsync("users"));
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetUsersAsync error: {response.StatusCode} - {errorContent}");
                return new List<User>();
            }
            return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetUsersAsync exception: {ex.Message}");
            return new List<User>();
        }
    }
        
    virtual public async Task<User> GetUserByIdAsync(int id)
    {
        try
        {
            var response = await AuthorizedRequestAsync(() => httpClient.GetAsync($"users/{id}"));
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get user: {response.StatusCode} - {errorContent}");
            }
            var user = await response.Content.ReadFromJsonAsync<User>();
            return user ?? throw new Exception("User wasn't found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetUserByIdAsync error: {ex.Message}");
            throw;
        }
    }
        
    //virtual public async Task<User> GetUserByEmailAsync(string email) =>
    //    await httpClient.GetFromJsonAsync<User>($"users/byemail/{email}") ?? throw new Exception("User wasn't found");

    virtual public async Task AddUserAsync(User user) {
        var response = await httpClient.PostAsJsonAsync<User>("users", user);
        if(response.StatusCode == System.Net.HttpStatusCode.Conflict) {
            throw new InvalidOperationException("UsernameAlreadyUsed");
        }
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    public async Task UpdateUserAsync(User newUser) {
        var response = await AuthorizedRequestAsync(() =>
            httpClient.PutAsJsonAsync<User>($"users/{newUser.Id}", newUser));
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    public async Task DeleteUserAsync(int id) {
        var response = await AuthorizedRequestAsync(() => httpClient.DeleteAsync($"users/{id}"));
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
