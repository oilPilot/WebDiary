namespace WebDiary.DTO;

// simple DTO returning user identifier and activity metrics used by admin pages
public record class UserActivityDTO
{
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public int EntriesCount { get; init; }
}