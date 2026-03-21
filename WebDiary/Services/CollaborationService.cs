using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Services;

public class CollaborationService : ICollaborationService
{
    private readonly DiariesContext dbContext;
    private readonly Serilog.ILogger logger;

    public CollaborationService(DiariesContext dbContext)
    {
        this.dbContext = dbContext;
        logger = Log.ForContext<CollaborationService>();
    }

    public async Task<CollaborationListDTO?> GetGroupCollaborators(int groupId, int userId)
    {
        try
        {
            var group = await dbContext.diaryGroups
                .Include(g => g.Permissions)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return null;

            // Check if user is owner or has access
            var userRole = await GetUserRoleInGroup(groupId, userId);
            if (userRole == null && group.UserId != userId)
                return null;

            var currentUserRole = group.UserId == userId ? "Owner" : userRole?.ToString();

            var collaborators = group.Permissions
                .Select(p => new GroupPermissionDTO
                {
                    Id = p.Id,
                    GroupId = p.GroupId,
                    UserId = p.UserId,
                    UserName = p.User?.UserName,
                    Role = p.Role,
                    CreatedAtUtc = p.CreatedAtUtc,
                    UpdatedAtUtc = p.UpdatedAtUtc
                })
                .ToList();

            return new CollaborationListDTO
            {
                GroupId = groupId,
                GroupName = group.Name,
                CurrentUserRole = currentUserRole,
                Collaborators = collaborators
            };
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting group collaborators for group {GroupId}", groupId);
            return null;
        }
    }

    public async Task<GroupPermissionDTO?> AddCollaborator(int groupId, int userId, ManageCollaborationDTO dto)
    {
        try
        {
            var group = await dbContext.diaryGroups.FindAsync(groupId);
            if (group == null)
                return null;

            // Only owner can add collaborators
            if (group.UserId != userId)
                return null;

            // Find the user by username
            var collaborator = await dbContext.users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (collaborator == null)
                return null;

            // Check if already has permission
            var existingPermission = await dbContext.groupPermissions
                .FirstOrDefaultAsync(p => p.GroupId == groupId && p.UserId == collaborator.Id);
            
            if (existingPermission != null)
                return null; // Already has permission

            var permission = new GroupPermission
            {
                GroupId = groupId,
                UserId = collaborator.Id,
                GrantedByUserId = userId,
                Role = dto.Role,
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.groupPermissions.Add(permission);
            await dbContext.SaveChangesAsync();

            return new GroupPermissionDTO
            {
                Id = permission.Id,
                GroupId = permission.GroupId,
                UserId = permission.UserId,
                UserName = collaborator.UserName,
                Role = permission.Role,
                CreatedAtUtc = permission.CreatedAtUtc,
                UpdatedAtUtc = permission.UpdatedAtUtc
            };
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error adding collaborator to group {GroupId}", groupId);
            return null;
        }
    }

    public async Task<GroupPermissionDTO?> UpdateCollaboratorRole(int groupId, int userId, string userName, UpdateCollaborationDTO dto)
    {
        try
        {
            var group = await dbContext.diaryGroups.FindAsync(groupId);
            if (group == null)
                return null;

            // Only owner can update roles
            if (group.UserId != userId)
                return null;

            var collaborator = await dbContext.users
                .FirstOrDefaultAsync(u => u.UserName == userName);
            if (collaborator == null)
                return null;

            var permission = await dbContext.groupPermissions
                .FirstOrDefaultAsync(p => p.GroupId == groupId && p.UserId == collaborator.Id);
            
            if (permission == null)
                return null;

            permission.Role = dto.Role;
            permission.UpdatedAtUtc = DateTime.UtcNow;

            dbContext.groupPermissions.Update(permission);
            await dbContext.SaveChangesAsync();

            return new GroupPermissionDTO
            {
                Id = permission.Id,
                GroupId = permission.GroupId,
                UserId = permission.UserId,
                UserName = collaborator.UserName,
                Role = permission.Role,
                CreatedAtUtc = permission.CreatedAtUtc,
                UpdatedAtUtc = permission.UpdatedAtUtc
            };
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating collaborator role in group {GroupId}", groupId);
            return null;
        }
    }

    public async Task<bool> RemoveCollaborator(int groupId, int userId, string userName)
    {
        try
        {
            var group = await dbContext.diaryGroups.FindAsync(groupId);
            if (group == null)
                return false;

            // Only owner can remove collaborators
            if (group.UserId != userId)
                return false;

            var collaborator = await dbContext.users
                .FirstOrDefaultAsync(u => u.UserName == userName);
            if (collaborator == null)
                return false;

            var permission = await dbContext.groupPermissions
                .FirstOrDefaultAsync(p => p.GroupId == groupId && p.UserId == collaborator.Id);
            
            if (permission == null)
                return false;

            dbContext.groupPermissions.Remove(permission);
            await dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error removing collaborator from group {GroupId}", groupId);
            return false;
        }
    }

    public async Task<bool> HasAccessToGroup(int groupId, int userId)
    {
        try
        {
            var group = await dbContext.diaryGroups.FindAsync(groupId);
            if (group == null)
                return false;

            // Owner has access
            if (group.UserId == userId)
                return true;

            // Check if has permission
            var permission = await dbContext.groupPermissions
                .FirstOrDefaultAsync(p => p.GroupId == groupId && p.UserId == userId);

            return permission != null;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error checking access to group {GroupId} for user {UserId}", groupId, userId);
            return false;
        }
    }

    public async Task<bool> CanEditInGroup(int groupId, int userId)
    {
        try
        {
            var group = await dbContext.diaryGroups.FindAsync(groupId);
            if (group == null)
                return false;

            // Owner can always edit
            if (group.UserId == userId)
                return true;

            // Check if has editor role
            var permission = await dbContext.groupPermissions
                .FirstOrDefaultAsync(p => p.GroupId == groupId && p.UserId == userId);

            return permission != null && permission.Role == GroupRole.Editor;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error checking edit permission in group {GroupId} for user {UserId}", groupId, userId);
            return false;
        }
    }

    public async Task<GroupRole?> GetUserRoleInGroup(int groupId, int userId)
    {
        try
        {
            var permission = await dbContext.groupPermissions
                .FirstOrDefaultAsync(p => p.GroupId == groupId && p.UserId == userId);

            return permission?.Role;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting user role in group {GroupId} for user {UserId}", groupId, userId);
            return null;
        }
    }
}
