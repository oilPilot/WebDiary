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

public static class DiaryEndpoints
{
    const string getDiaryRoute = "diaryEndpoint";

    public static RouteGroupBuilder AddDiariesEndpoints(this WebApplication app) {
        // using group as it is easier to think
        var group = app.MapGroup("diaries").RequireAuthorization();

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
                return Results.Ok(await dbContext.diaries
                    .Include(d => d.Owner)
                    .Select(diary => diary.ToDTO())
                    .AsNoTracking()
                    .ToListAsync());
            }

            // Get groups owned by user
            var ownedGroupIds = dbContext.diaryGroups
                .Where(group => group.UserId == userId.Value)
                .Select(group => group.Id);

            // Get groups where user has any permission (editor or viewer)
            var sharedGroupIds = dbContext.groupPermissions
                .Where(p => p.UserId == userId.Value)
                .Select(p => p.GroupId);

            var allGroupIds = ownedGroupIds.Union(sharedGroupIds);

            return Results.Ok(await dbContext.diaries
                .Where(diary => allGroupIds.Contains(diary.GroupId))
                .Include(d => d.Owner)
                .Select(diary => diary.ToDTO())
                .AsNoTracking()
                .ToListAsync());
        });
        group.MapGet("/ofgroup/{groupId:int}", async (int groupId, ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            var ownsGroup = await CanAccessGroup(principal, dbContext, groupId);
            if (!ownsGroup)
            {
                return Results.Forbid();
            }

            return Results.Ok(await dbContext.diaries
                .Where(diary => diary.GroupId == groupId)
                .Include(d => d.Owner)
                .Select(diary => diary.ToDTO())
                .AsNoTracking()
                .ToListAsync());
        });
        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal principal, DiariesContext dbContext) => {
            var diary = await dbContext.diaries.Include(d => d.Owner).AsNoTracking().FirstOrDefaultAsync(diary => diary.Id == id);
            if(diary is null) {
                Serilog.Log.Error("Search of diary by id '{ID}' was unsuccessful", id);
                return Results.NotFound();
            }
            var ownsGroup = await CanAccessGroup(principal, dbContext, diary.GroupId);
            if(!ownsGroup)
            {
                return Results.Forbid();
            }

            return Results.Ok(diary.ToDTO());
            }).WithName(getDiaryRoute);
        
        // mapping POST methods
        group.MapPost("/", async (CreateDiaryDTO createDiary, ClaimsPrincipal principal, DiariesContext dbContext) => {
            try {
                var userId = GetCurrentUserId(principal);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var canAccess = await CanAccessGroupForWriting(principal, dbContext, createDiary.GroupId);
                if(!canAccess)
                {
                    return Results.Forbid();
                }

                Diary diary = createDiary.ToEntity();
                diary.OwnerId = userId.Value;
                diary.CreatedUtc = createDiary.CreatedUtc;
                diary.UtcOffsetMinutes = createDiary.UtcOffsetMinutes;
                
                await dbContext.diaries.AddAsync(diary);
                await dbContext.SaveChangesAsync();
                Serilog.Log.Information("Added diary with name: '{Name}' to group with id: '{Id}' by user '{UserId}'", 
                    diary.Text, diary.GroupId, userId);

                return Results.CreatedAtRoute(getDiaryRoute, new {id = diary.Id}, diary.ToDTO());
            } catch (Exception Ex) {
                Serilog.Log.Fatal("Adding Diary was failed. Creating diary data: " +
                "{@creatingDiary} Exception text: {Exception}", createDiary, Ex);
                return Results.Problem("Unexpected error while adding diary.");
            }
        });

        /* mapping PUT methods
        Updating for diaries shouldn't be possible
        
        group.MapPut("/{id}", (int id) => {
            return Results.BadRequest("You can't change diaries");
        });

        /* mapping DELETE methods
        Deleting diaries shouldn't be possible
        Up until AdminPanel was created */
        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal principal, DiariesContext dbContext) => {
            var diary = await dbContext.diaries.AsNoTracking().FirstOrDefaultAsync(diary => diary.Id == id);
            if(diary is null)
            {
                return Results.NotFound();
            }
            var ownsGroup = await CanAccessGroup(principal, dbContext, diary.GroupId);
            if(!ownsGroup)
            {
                return Results.Forbid();
            }

            await dbContext.diaries.Where(diary => diary.Id == id).ExecuteDeleteAsync();
            return Results.NoContent();
        });

        // Entry Reference endpoints - for referencing other entries
        group.MapGet("/{id:int}/references", async (int id, ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            var diary = await dbContext.diaries.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (diary is null)
            {
                return Results.NotFound("Entry not found.");
            }

            var canAccess = await CanAccessGroup(principal, dbContext, diary.GroupId);
            if (!canAccess)
            {
                return Results.Forbid();
            }

            var references = await dbContext.entryReferences
                .Where(r => r.SourceEntryId == id)
                .Include(r => r.ReferencedEntry)
                .ThenInclude(d => d.Owner)
                .AsNoTracking()
                .Select(r => r.toDTO())
                .ToListAsync();

            return Results.Ok(references);
        });

        group.MapPost("/{id:int}/references", async (int id, CreateEntryReferenceDTO referenceDTO, ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            try
            {
                var sourceEntry = await dbContext.diaries.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                if (sourceEntry is null)
                {
                    return Results.NotFound("Source entry not found.");
                }

                var referencedEntry = await dbContext.diaries.AsNoTracking().FirstOrDefaultAsync(d => d.Id == referenceDTO.ReferencedEntryId);
                if (referencedEntry is null)
                {
                    return Results.NotFound("Referenced entry not found.");
                }

                // Check access to both entries
                var canAccessSource = await CanAccessGroup(principal, dbContext, sourceEntry.GroupId);
                var canAccessTarget = await CanAccessGroup(principal, dbContext, referencedEntry.GroupId);

                if (!canAccessSource || !canAccessTarget)
                {
                    return Results.Problem("You don't have access to one or both entries.");
                }

                // Check if user can write to the source entry (is owner or has editor role)
                var userId = GetCurrentUserId(principal);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                // You can only add references to your own entries or if you're an owner/editor
                var canEditSource = await dbContext.diaryGroups.AnyAsync(g => g.Id == sourceEntry.GroupId && g.UserId == userId.Value) ||
                    await dbContext.groupPermissions.AnyAsync(p =>
                        p.GroupId == sourceEntry.GroupId &&
                        p.UserId == userId.Value &&
                        (p.Role == GroupRole.Editor || p.Role == GroupRole.Owner));

                if (!canEditSource)
                {
                    return Results.Problem("You cannot add references to this entry.");
                }

                var reference = referenceDTO.toEntity();
                reference.SourceEntryId = id;
                reference.CreatedAtUtc = DateTime.UtcNow;

                await dbContext.entryReferences.AddAsync(reference);
                await dbContext.SaveChangesAsync();

                Serilog.Log.Information("Created reference from entry {SourceId} to entry {TargetId}", id, referenceDTO.ReferencedEntryId);

                // Fetch the created reference with the referenced entry details
                var createdReference = await dbContext.entryReferences
                    .Where(r => r.Id == reference.Id)
                    .Include(r => r.ReferencedEntry)
                    .ThenInclude(d => d.Owner)
                    .AsNoTracking()
                    .Select(r => r.toDTO())
                    .FirstAsync();

                return Results.Created($"/diaries/{id}/references/{reference.Id}", createdReference);
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal("Failed to create entry reference. Exception: {Exception}", ex);
                return Results.Problem("Unexpected error while creating reference.");
            }
        });

        group.MapDelete("/{id:int}/references/{referenceId:int}", async (int id, int referenceId, ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            try
            {
                var reference = await dbContext.entryReferences.FirstOrDefaultAsync(r => r.Id == referenceId && r.SourceEntryId == id);
                if (reference is null)
                {
                    return Results.NotFound("Reference not found.");
                }

                var sourceEntry = await dbContext.diaries.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                if (sourceEntry is null)
                {
                    return Results.NotFound("Entry not found.");
                }

                var userId = GetCurrentUserId(principal);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                // Only the creator of the source entry or admins can delete references
                if (sourceEntry.OwnerId != userId && !principal.IsInRole("Admin"))
                {
                    return Results.Problem("You cannot delete this reference.");
                }

                dbContext.entryReferences.Remove(reference);
                await dbContext.SaveChangesAsync();

                Serilog.Log.Information("Deleted reference {ReferenceId} from entry {EntryId}", referenceId, id);

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal("Failed to delete entry reference. Exception: {Exception}", ex);
                return Results.Problem("Unexpected error while deleting reference.");
            }
        });

        return group;
    }

    private static int? GetCurrentUserId(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirstValue("userId") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static async Task<bool> CanAccessGroup(ClaimsPrincipal principal, DiariesContext dbContext, int groupId)
    {
        if (principal.IsInRole("Admin"))
        {
            return await dbContext.diaryGroups.AnyAsync(group => group.Id == groupId);
        }

        var userId = GetCurrentUserId(principal);
        if (userId is null)
        {
            return false;
        }

        // Check if user owns the group
        var ownsGroup = await dbContext.diaryGroups.AnyAsync(group => group.Id == groupId && group.UserId == userId.Value);
        if (ownsGroup)
        {
            return true;
        }

        // Check if user has permission in the group (viewer, editor, or owner)
        return await dbContext.groupPermissions.AnyAsync(p => p.GroupId == groupId && p.UserId == userId.Value);
    }

    private static async Task<bool> CanAccessGroupForWriting(ClaimsPrincipal principal, DiariesContext dbContext, int groupId)
    {
        if (principal.IsInRole("Admin"))
        {
            return await dbContext.diaryGroups.AnyAsync(group => group.Id == groupId);
        }

        var userId = GetCurrentUserId(principal);
        if (userId is null)
        {
            return false;
        }

        // Owner can always write
        var isOwner = await dbContext.diaryGroups.AnyAsync(group => group.Id == groupId && group.UserId == userId.Value);
        if (isOwner)
        {
            return true;
        }

        // Check if user has editor or owner permission (not viewer)
        return await dbContext.groupPermissions.AnyAsync(p => 
            p.GroupId == groupId && 
            p.UserId == userId.Value && 
            (p.Role == GroupRole.Editor || p.Role == GroupRole.Owner));
    }
}
