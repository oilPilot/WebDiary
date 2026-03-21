using System;

namespace WebDiary.Frontend.Models;

/// <summary>
/// Represents a user's permission level in a diary group
/// </summary>
public class GroupPermission
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int Role { get; set; } // 0 = Owner, 1 = Editor, 2 = Viewer
    public int GrantedByUserId { get; set; }
    public string? GrantedByUserName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
