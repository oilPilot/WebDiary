using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Entities;
using WebDiary.Mapping;

namespace WebDiary.Endpoints;

public static class GroupEndpoints
{
    const string getGroupRoute = "groupEndpoint";

    public static RouteGroupBuilder AddGroupsEndpoints(this WebApplication app) {
        // using group as it is easier to think
        var group = app.MapGroup("groups").RequireAuthorization();

        // mapping GET methods
        group.MapGet("/", async (ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            var userId = GetCurrentUserId(principal);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            if (principal.IsInRole("Admin"))
            {
                return Results.Ok(await dbContext.diaryGroups
                    .Where(diaryGroup => !diaryGroup.IsArchived)
                    .Select(diaryGroup => diaryGroup.toDTO())
                    .AsNoTracking()
                    .ToListAsync());
            }

            return Results.Ok(await dbContext.diaryGroups
                .Where(diaryGroup => diaryGroup.UserId == userId.Value && !diaryGroup.IsArchived)
                .Select(diaryGroup => diaryGroup.toDTO())
                .AsNoTracking()
                .ToListAsync());
        });
        group.MapGet("/archived", async (ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            var userId = GetCurrentUserId(principal);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            if (principal.IsInRole("Admin"))
            {
                return Results.Ok(await dbContext.diaryGroups
                    .Where(diaryGroup => diaryGroup.IsArchived)
                    .Select(diaryGroup => diaryGroup.toDTO())
                    .AsNoTracking()
                    .ToListAsync());
            }

            return Results.Ok(await dbContext.diaryGroups
                .Where(diaryGroup => diaryGroup.UserId == userId.Value && diaryGroup.IsArchived)
                .Select(diaryGroup => diaryGroup.toDTO())
                .AsNoTracking()
                .ToListAsync());
        });
        group.MapGet("/ofuser/{userId:int}", async (int userId, ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            if (!CanAccessUserGroups(principal, userId))
            {
                return Results.Forbid();
            }

            return Results.Ok(await dbContext.diaryGroups
                .Where(diaryGroup => !diaryGroup.IsArchived &&
                    (diaryGroup.UserId == userId ||
                     dbContext.groupPermissions.Any(p => p.GroupId == diaryGroup.Id && p.UserId == userId)))
                .Include(diaryGroup => diaryGroup.Owner)
                .Select(diaryGroup => diaryGroup.toDTO())
                .AsNoTracking()
                .ToListAsync());
        });
        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal principal, DiariesContext dbContext) => {
            var group = await dbContext.diaryGroups.AsNoTracking().FirstOrDefaultAsync(group => group.Id == id);
            if (group is null) {
                Serilog.Log.Error("Search of group by id '{ID}' was unsuccessful", id);
                return Results.NotFound();
            }
            if(!CanAccessUserGroups(principal, group.UserId))
            {
                return Results.Forbid();
            }

            return Results.Ok(group.toDTO());
        }).WithName(getGroupRoute);
        
        // mapping POST methods
        group.MapPost("/", async (CreateGroupDTO newGroup, ClaimsPrincipal principal, DiariesContext dbContext) => {
            try {
                var userId = GetCurrentUserId(principal);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var ownerId = principal.IsInRole("Admin") && newGroup.UserId > 0 ? newGroup.UserId : userId.Value;
                var payload = new CreateGroupDTO
                {
                    Name = newGroup.Name,
                    UserId = ownerId,
                    PinCode = newGroup.PinCode
                };
                var group = payload.toEntity();

                await dbContext.diaryGroups.AddAsync(group);
                await dbContext.SaveChangesAsync();

                Serilog.Log.Information("Created new group with name: {Name} and id: {Id}", group.Name, group.Id);

                return Results.CreatedAtRoute(getGroupRoute, new {id = group.Id}, group.toDTO());
            } catch (Exception Ex) {
                Serilog.Log.Fatal("Adding Group was failed. New Group data: " +
                "{@newGroup} Exception text: {Exception}", newGroup, Ex);
                return Results.Problem("Unexpected error while creating group.");
            }
        });

        // mapping PUT methods
        group.MapPut("/{id:int}", async (int id, UpdateGroupDTO newGroup, ClaimsPrincipal principal, DiariesContext dbContext) => {
            try {
                var currentGroup = await dbContext.diaryGroups.FindAsync(id);
                if(currentGroup is null) {
                    Serilog.Log.Error("Search of group by id '{ID}' upon updating was unsuccessful", id);
                    return Results.NotFound();
                }
                if(!CanAccessUserGroups(principal, currentGroup.UserId))
                {
                    return Results.Forbid();
                }

                var group = newGroup.toEntity(id, currentGroup.UserId);
                dbContext.diaryGroups.Entry(currentGroup).CurrentValues.SetValues(group);
                await dbContext.SaveChangesAsync();
                
                return Results.NoContent();
            } catch (Exception Ex) {
                Serilog.Log.Fatal("Updating Group was failed. New Group data: " +
                "{@newGroup} Exception text: {Exception}", newGroup, Ex);
                return Results.Problem("Unexpected error while updating group.");
            }
        });

        // mapping DELETE methods
        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal principal, DiariesContext dbContext) => {
            var currentGroup = await dbContext.diaryGroups.AsNoTracking().FirstOrDefaultAsync(group => group.Id == id);
            if(currentGroup is null)
            {
                return Results.NotFound();
            }
            if(!CanAccessUserGroups(principal, currentGroup.UserId))
            {
                return Results.Forbid();
            }

            await dbContext.diaries.Where(diary => diary.GroupId == id).ExecuteDeleteAsync();
            await dbContext.diaryGroups.Where(group => group.Id == id).ExecuteDeleteAsync();

            Serilog.Log.Information("Deleted group with id '{ID}' and it's diaries", id);

            return Results.NoContent();
        });

        // Collaboration endpoints - manage group members and permissions
        group.MapGet("/{id:int}/members", async (int id, ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            var userId = GetCurrentUserId(principal);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var groupPermission = await dbContext.groupPermissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.GroupId == id && p.UserId == userId);

            // Only owner and editors can see the member list
            if (groupPermission is null || groupPermission.Role == GroupRole.Viewer)
            {
                // Allow viewing if owner of the group
                var group = await dbContext.diaryGroups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
                if (group is null || group.UserId != userId)
                {
                    return Results.Forbid();
                }
            }

            var members = await dbContext.groupPermissions
                .Where(p => p.GroupId == id)
                .Include(p => p.User)
                .Include(p => p.GrantedByUser)
                .AsNoTracking()
                .Select(p => p.toDTO())
                .ToListAsync();

            return Results.Ok(members);
        });

        group.MapPost("/{id:int}/members", async (int id, ManageCollaborationDTO collaboration, ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            try
            {
                var userId = GetCurrentUserId(principal);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var currentGroup = await dbContext.diaryGroups.FirstOrDefaultAsync(g => g.Id == id);
                if (currentGroup is null)
                {
                    return Results.NotFound($"Group with id {id} not found.");
                }

                // Only owner can add members
                if (currentGroup.UserId != userId && !principal.IsInRole("Admin"))
                {
                    return Results.Forbid();
                }

                // Find the user by username
                var targetUser = await dbContext.users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserName == collaboration.UserName);
                if (targetUser is null)
                {
                    return Results.BadRequest($"User with username '{collaboration.UserName}' not found.");
                }

                // Check if permission already exists
                var existingPermission = await dbContext.groupPermissions
                    .FirstOrDefaultAsync(p => p.GroupId == id && p.UserId == targetUser.Id);
                if (existingPermission is not null)
                {
                    return Results.BadRequest($"User '{collaboration.UserName}' is already a member of this group.");
                }

                var permission = new GroupPermission
                {
                    GroupId = id,
                    UserId = targetUser.Id,
                    GrantedByUserId = userId.Value,
                    Role = collaboration.Role,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await dbContext.groupPermissions.AddAsync(permission);
                await dbContext.SaveChangesAsync();

                Serilog.Log.Information("Added user '{UserName}' to group '{GroupId}' with role '{Role}'",
                    collaboration.UserName, id, collaboration.Role);

                return Results.Created($"/groups/{id}/members/{targetUser.Id}", permission.toDTO());
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal("Failed to add member to group. Collaboration data: {@Data}, Exception: {Exception}",
                    collaboration, ex);
                return Results.Problem("Unexpected error while adding member to group.");
            }
        });

        group.MapPut("/{id:int}/members/{targetUserId:int}", async (int id, int targetUserId, UpdateCollaborationDTO collaboration, ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            try
            {
                var userId = GetCurrentUserId(principal);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var currentGroup = await dbContext.diaryGroups.FirstOrDefaultAsync(g => g.Id == id);
                if (currentGroup is null)
                {
                    return Results.NotFound($"Group with id {id} not found.");
                }

                // Only owner can update permissions
                if (currentGroup.UserId != userId && !principal.IsInRole("Admin"))
                {
                    return Results.Forbid();
                }

                var permission = await dbContext.groupPermissions
                    .FirstOrDefaultAsync(p => p.GroupId == id && p.UserId == targetUserId);
                if (permission is null)
                {
                    return Results.NotFound($"Member not found in this group.");
                }

                // Cannot change owner's role
                if (permission.Role == GroupRole.Owner)
                {
                    return Results.BadRequest("Cannot modify owner's permissions.");
                }

                permission.Role = collaboration.Role;
                permission.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();

                Serilog.Log.Information("Updated user '{UserId}' role in group '{GroupId}' to '{Role}'",
                    targetUserId, id, collaboration.Role);

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal("Failed to update member role. Exception: {Exception}", ex);
                return Results.Problem("Unexpected error while updating member role.");
            }
        });

        group.MapDelete("/{id:int}/members/{targetUserId:int}", async (int id, int targetUserId, ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            try
            {
                var userId = GetCurrentUserId(principal);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var currentGroup = await dbContext.diaryGroups.FirstOrDefaultAsync(g => g.Id == id);
                if (currentGroup is null)
                {
                    return Results.NotFound($"Group with id {id} not found.");
                }

                // Only owner can remove members
                if (currentGroup.UserId != userId && !principal.IsInRole("Admin"))
                {
                    return Results.Forbid();
                }

                var permission = await dbContext.groupPermissions
                    .FirstOrDefaultAsync(p => p.GroupId == id && p.UserId == targetUserId);
                if (permission is null)
                {
                    return Results.NotFound($"Member not found in this group.");
                }

                // Cannot remove owner
                if (permission.Role == GroupRole.Owner)
                {
                    return Results.BadRequest("Cannot remove the owner from the group.");
                }

                dbContext.groupPermissions.Remove(permission);
                await dbContext.SaveChangesAsync();

                Serilog.Log.Information("Removed user '{UserId}' from group '{GroupId}'", targetUserId, id);

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal("Failed to remove member from group. Exception: {Exception}", ex);
                return Results.Problem("Unexpected error while removing member from group.");
            }
        });

        return group;
    }

    private static int? GetCurrentUserId(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirstValue("userId") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static bool CanAccessUserGroups(ClaimsPrincipal principal, int ownerUserId)
    {
        if (principal.IsInRole("Admin"))
        {
            return true;
        }

        return GetCurrentUserId(principal) == ownerUserId;
    }
}
