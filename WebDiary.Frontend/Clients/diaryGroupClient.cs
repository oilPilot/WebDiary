using System;
using WebDiary.Frontend.Models;

namespace WebDiary.Frontend.Clients;

public class DiaryGroupClient(HttpClient httpClient)
{
    public async Task<List<DiaryGroup>> GetGroupsAsync() =>
        await httpClient.GetFromJsonAsync<List<DiaryGroup>>("groups") ?? new List<DiaryGroup>();
        
    public async Task<List<DiaryGroup>> GetGroupsOfUserAsync(int userId) =>
        await httpClient.GetFromJsonAsync<List<DiaryGroup>>($"groups/ofuser/{userId}") ?? new List<DiaryGroup>();
        
    public async Task<DiaryGroup> GetGroupAsync(int id) =>
        await httpClient.GetFromJsonAsync<DiaryGroup>($"groups/{id}") ?? throw new Exception("Diary wasn't found");

    public async Task AddGroupAsync(DiaryGroup group) =>
        await httpClient.PostAsJsonAsync<DiaryGroup>("groups", group);

    public async Task UpdateGroupAsync(DiaryGroup newGroup) =>
        await httpClient.PutAsJsonAsync<DiaryGroup>($"groups/{newGroup.Id}", newGroup);

    public async Task DeleteGroupAsync(int id) =>
        await httpClient.DeleteAsync($"groups/{id}");
}
