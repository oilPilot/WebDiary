using System;

namespace WebDiary.Frontend.Models;

public class DiaryGroup
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public required string OwnerUserName { get; set; }
    public int OwnerId { get; set; }
    public string? PinCode { get; set; }
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
