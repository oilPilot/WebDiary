using System;
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
        var group = app.MapGroup("groups");

        // mapping GET methods
        group.MapGet("/", async (DiariesContext dbContext) => await dbContext.diaryGroups.Select(diaryGroup => diaryGroup.toDTO()).AsNoTracking().ToListAsync());
        group.MapGet("/ofuser/{userId}", async (int userId, DiariesContext dbContext) =>
            await dbContext.diaryGroups.Where(group => group.UserId == userId).Select(group => group.toDTO()).AsNoTracking().ToListAsync());
        group.MapGet("/{id}", async (int id, DiariesContext dbContext) => {
            var group = await dbContext.diaryGroups.FindAsync(id);
            if(group is null) {
                Log.Error("Search of group by id '{ID}' was unsuccessful", id);
                return Results.NotFound();
            }

            return Results.Ok(group.toDTO());
        }).WithName(getGroupRoute);
        
        // mapping POST methods
        group.MapPost("/", async (CreateGroupDTO newGroup, DiariesContext dbContext) => {
            var group = newGroup.toEntity();

            await dbContext.diaryGroups.AddAsync(group);
            await dbContext.SaveChangesAsync();

            Log.Information("Created new group with name: {Name} and id: {Id}", group.Name, group.Id);

            return Results.CreatedAtRoute(getGroupRoute, new {id = group.Id}, group.toDTO());
        });

        // mapping PUT methods
        group.MapPut("/{id}", async (int id, UpdateGroupDTO newGroup, DiariesContext dbContext) => {
            var currentGroup = await dbContext.diaryGroups.FindAsync(id);
            if(currentGroup is null) {
                Log.Error("Search of group by id '{ID}' upon updating was unsuccessful", id);
                return Results.NotFound();
            }

            var group = newGroup.toEntity(id, currentGroup.UserId);
            dbContext.diaryGroups.Entry(currentGroup).CurrentValues.SetValues(group);
            await dbContext.SaveChangesAsync();
            
            return Results.NoContent();
        });

        // mapping DELETE methods
        group.MapDelete("/{id}", async (int id, DiariesContext dbContext) => {
            await dbContext.diaryGroups.Where(group => group.Id == id).ExecuteDeleteAsync();
            await dbContext.diaries.Where(diary => diary.GroupId == id).ExecuteDeleteAsync();

            Log.Information("Deleted group with id '{ID}' and it's diaries", id);

            return Results.NoContent();
        });

        return group;
    }
}
