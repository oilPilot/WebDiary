using System.ComponentModel.DataAnnotations;

namespace WebDiary.DTO;

public record class DiaryDTO
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public int GroupId { get; set; }
    public required string mood { get; set; }
    public int OwnerId { get; set; }
    public string? OwnerUserName { get; set; }
}

public record class CreateDiaryDTO
{
    [Required]
    public required string Text { get; set; }
    [Required]
    public int GroupId { get; set; } = 1;
    [Required]
    public required string mood { get; set; }
    [Required]
    public DateTime CreatedUtc { get; set; }
    [Required]
    public int UtcOffsetMinutes { get; set; }
}
