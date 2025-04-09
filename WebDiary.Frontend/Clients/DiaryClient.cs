using System;
using WebDiary.Frontend.Models;

namespace WebDiary.Frontend.Clients;

public class DiaryClient(HttpClient httpClient)
{
    public async Task<List<Diary>> GetDiariesAsync() =>
        await httpClient.GetFromJsonAsync<List<Diary>>("diaries") ?? new List<Diary>();
    public async Task<List<Diary>> GetDiariesOfGroupAsync(int groupId) =>
        await httpClient.GetFromJsonAsync<List<Diary>>($"diaries/ofgroup/{groupId}") ?? new List<Diary>();
        
    public async Task<Diary> GetDiaryAsync(int id) =>
        await httpClient.GetFromJsonAsync<Diary>($"diaries/{id}") ?? throw new Exception("Diary wasn't found");

    public async Task AddDiaryAsync(Diary diary) =>
        await httpClient.PostAsJsonAsync<Diary>("diaries", diary);

    public async Task UpdateDiaryAsync(Diary newDiary) =>
        await httpClient.PutAsJsonAsync<Diary>($"diaries/{newDiary.Id}", newDiary);

    public async Task DeleteDiaryAsync(int id) =>
        await httpClient.DeleteAsync($"diaries/{id}");
}
