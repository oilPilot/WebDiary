using System;
using System.Collections.Generic;

namespace WebDiary.Entities;

public class DiaryGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int UserId { get; set; }
    public User? Owner { get; set; }
    public string? PinCode { get; set; }
    public bool IsArchived { get; set; } = false;
    
    // Timestamps
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    
    // Navigation properties for permissions
    public ICollection<GroupPermission> Permissions { get; set; } = new List<GroupPermission>();
}
