using System;
using System.Collections.Generic;

namespace WebDiary.Frontend.Models;

/// <summary>
/// Request to add a collaborator to a group
/// </summary>
public class ManageCollaborationRequest
{
    public required string Username { get; set; }
    public int Role { get; set; } // 1 = Editor, 2 = Viewer
}

/// <summary>
/// Request to update a collaborator's role
/// </summary>
public class UpdateCollaborationRequest
{
    public int Role { get; set; }
}

/// <summary>
/// Response containing list of collaborators
/// </summary>
public class CollaborationListResponse
{
    public List<GroupPermission>? Members { get; set; } = new();
}
