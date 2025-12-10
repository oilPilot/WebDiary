using System;

namespace WebDiary.Entities;

public class DiaryGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int UserId { get; set; }
    public string? PinCode { get; set; }
    // Maybe i would want to add later creationDate
}
