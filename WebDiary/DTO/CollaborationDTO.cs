using System.ComponentModel.DataAnnotations;
using WebDiary.Entities;

namespace WebDiary.DTO;

/// <summary>
/// DTO for displaying group permissions
/// </summary>
public record class GroupPermissionDTO
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public GroupRole Role { get; set; }
    public string? GrantedByUserName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

/// <summary>
/// DTO for creating new group permissions or adding users to groups
/// </summary>
public record class ManageCollaborationDTO
{
    [Required]
    public required string UserName { get; set; }
    [Required]
    public GroupRole Role { get; set; }
}

/// <summary>
/// DTO for updating existing group permissions
/// </summary>
public record class UpdateCollaborationDTO
{
    [Required]
    public GroupRole Role { get; set; }
}

/// <summary>
/// DTO for removing user from group collaboration
/// </summary>
public record class RemoveCollaborationDTO
{
    [Required]
    public required string UserName { get; set; }
}

/// <summary>
/// DTO for listing all collaborators in a group
/// </summary>
public record class CollaborationListDTO
{
    public int GroupId { get; set; }
    public string? GroupName { get; set; }
    public string? CurrentUserRole { get; set; }
    public List<GroupPermissionDTO>? Collaborators { get; set; }
}
