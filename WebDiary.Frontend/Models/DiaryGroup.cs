using System;

namespace WebDiary.Frontend.Models;

public class DiaryGroup
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? UserId { get; set; }
}
