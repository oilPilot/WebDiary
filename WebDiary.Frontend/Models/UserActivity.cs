namespace WebDiary.Frontend.Models;

public record class UserActivity
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int EntriesCount { get; set; }
}