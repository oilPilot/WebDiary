using System;

namespace WebDiary.Frontend.Models;

public class DiaryGroup
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? UserId { get; set; }
    public string? PinCode { get; set; }
    public bool IsArchived { get; set; } = false;
    // Maybe i would want to add later creationDate
}
