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
                return Results.Ok(await dbContext.diaries.Select(diary => diary.ToDTO()).AsNoTracking().ToListAsync());
            }

            var groupIds = dbContext.diaryGroups
                .Where(group => group.UserId == userId.Value)
                .Select(group => group.Id);

            return Results.Ok(await dbContext.diaries
                .Where(diary => groupIds.Contains(diary.GroupId))
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
                .Select(diary => diary.ToDTO())
                .AsNoTracking()
                .ToListAsync());
        });
        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal principal, DiariesContext dbContext) => {
            var diary = await dbContext.diaries.AsNoTracking().FirstOrDefaultAsync(diary => diary.Id == id);
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
                var ownsGroup = await CanAccessGroup(principal, dbContext, createDiary.GroupId);
                if(!ownsGroup)
                {
                    return Results.Forbid();
                }

                Diary diary = createDiary.ToEntity();
                await dbContext.diaries.AddAsync(diary);
                await dbContext.SaveChangesAsync();
                Serilog.Log.Information("Added diary with name: '{Name}' to group with id: '{Id}'", diary.Text, diary.GroupId);

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

        return await dbContext.diaryGroups.AnyAsync(group => group.Id == groupId && group.UserId == userId.Value);
    }
}
