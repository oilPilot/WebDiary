using System;

namespace WebDiary.Frontend.Models;

public class Diary
{
    public int Id { get; set; }
    public required string Text { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public int? UtcOffsetMinutes { get; set; }

    public required int GroupId { get; set; }
    public string? mood { get; set; }
}
