using System;

namespace WebDiary.Entities;

public enum GroupRole
{
    Owner = 0,
    Editor = 1,
    Viewer = 2
}

public class GroupPermission
{
    public int Id { get; set; }
    
    // Foreign keys
    public int GroupId { get; set; }
    public DiaryGroup? Group { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    // Who granted this permission
    public int GrantedByUserId { get; set; }
    public User? GrantedByUser { get; set; }
    
    // The role/permission level
    public GroupRole Role { get; set; }
    
    // Timestamps
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
