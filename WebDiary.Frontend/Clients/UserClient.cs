using System;
using WebDiary.Frontend.Models;

namespace WebDiary.Frontend.Clients;

public class UserClient(HttpClient httpClient)
{
    public async Task<List<User>> GetUsersAsync() =>
        await httpClient.GetFromJsonAsync<List<User>>("users") ?? new List<User>();
        
    public async Task<User> GetUserAsync(int id) =>
        await httpClient.GetFromJsonAsync<User>($"users/{id}") ?? throw new Exception("Diary wasn't found");
        
    public async Task<User> GetUserByEmailAsync(string email) =>
        await httpClient.GetFromJsonAsync<User>($"users/byemail/{email}") ?? throw new Exception("Diary wasn't found");

    public async Task AddUserAsync(User user) =>
        await httpClient.PostAsJsonAsync<User>("users", user);

    public async Task UpdateUserAsync(User newUser) =>
        await httpClient.PutAsJsonAsync<User>($"users/{newUser.Id}", newUser);

    public async Task DeleteUserAsync(int id) =>
        await httpClient.DeleteAsync($"users/{id}");
}
