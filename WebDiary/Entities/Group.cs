using System;

namespace WebDiary.Entities;

public class DiaryGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int UserId { get; set; }
}
