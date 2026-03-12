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
                .Where(diaryGroup => diaryGroup.UserId == userId && !diaryGroup.IsArchived)
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
