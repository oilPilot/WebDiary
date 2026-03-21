using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Services;

public interface ICollaborationService
{
    // Get all collaborators for a group
    Task<CollaborationListDTO?> GetGroupCollaborators(int groupId, int userId);
    
    // Add collaborator to group
    Task<GroupPermissionDTO?> AddCollaborator(int groupId, int userId, ManageCollaborationDTO dto);
    
    // Update collaborator role
    Task<GroupPermissionDTO?> UpdateCollaboratorRole(int groupId, int userId, string userName, UpdateCollaborationDTO dto);
    
    // Remove collaborator from group
    Task<bool> RemoveCollaborator(int groupId, int userId, string userName);
    
    // Check if user has permission to access group
    Task<bool> HasAccessToGroup(int groupId, int userId);
    
    // Check if user can edit in group
    Task<bool> CanEditInGroup(int groupId, int userId);
    
    // Get user's role in group
    Task<GroupRole?> GetUserRoleInGroup(int groupId, int userId);
}
