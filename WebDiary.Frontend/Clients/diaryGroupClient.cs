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
        await httpClient.GetFromJsonAsync<DiaryGroup>($"groups/{id}") ?? throw new Exception("Group wasn't found");

    public async Task AddGroupAsync(DiaryGroup group) {
        var response = await httpClient.PostAsJsonAsync<DiaryGroup>("groups", group);
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    public async Task UpdateGroupAsync(DiaryGroup newGroup) {
        var response = await httpClient.PutAsJsonAsync<DiaryGroup>($"groups/{newGroup.Id}", newGroup);
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }

    public async Task DeleteGroupAsync(int id) {
        var response = await httpClient.DeleteAsync($"groups/{id}");
        if(!response.IsSuccessStatusCode) {
            throw new Exception();
        }
    }
}
