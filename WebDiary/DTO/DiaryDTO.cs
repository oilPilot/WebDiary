using System.ComponentModel.DataAnnotations;

namespace WebDiary.DTO;

public record class DiaryDTO
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public int GroupId { get; set; }
}

public record class CreateDiaryDTO
{
    [Required]
    public required string Text { get; set; }
    [Required]
    public int GroupId { get; set; } = 1;
}
