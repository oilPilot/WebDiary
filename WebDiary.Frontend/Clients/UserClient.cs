using System;
using Microsoft.AspNetCore.Components.Authorization;
using WebDiary.Frontend.Models;
using WebDiary.Frontend.Models.Auth;

namespace WebDiary.Frontend.Clients;

public class UserClient(HttpClient httpClient)
{
    virtual public async Task<List<User>> GetUsersAsync() =>
        await httpClient.GetFromJsonAsync<List<User>>("users") ?? new List<User>();
        
    virtual public async Task<User> GetUserByIdAsync(int id) =>
        await httpClient.GetFromJsonAsync<User>($"users/{id}") ?? throw new Exception("User wasn't found");
        
    virtual public async Task<User> GetUserByEmailAsync(string email) =>
        await httpClient.GetFromJsonAsync<User>($"users/byemail/{email}") ?? throw new Exception("User wasn't found");

    public async Task AddUserAsync(User user) {
        var response = await httpClient.PostAsJsonAsync<User>("users", user);
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    public async Task UpdateUserAsync(User newUser) {
        var response = await httpClient.PutAsJsonAsync<User>($"users/{newUser.Id}", newUser);
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    public async Task DeleteUserAsync(int id) {
        var response = await httpClient.DeleteAsync($"users/{id}");
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }
}
