using System;
using System.Text.RegularExpressions;

namespace WebDiary.Entities;

public class Diary
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public int GroupId { get; set; }
    public DiaryGroup? Group { get; set; }
}
