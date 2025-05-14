using System;
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
        var group = app.MapGroup("diaries");

        // mapping GET methods
        group.MapGet("/", [Authorize] async (DiariesContext dbContext) =>
            await dbContext.diaries.Select(diary => diary.ToDTO()).AsNoTracking().ToListAsync());
        group.MapGet("/ofgroup/{groupId}", [Authorize] async (int groupId, DiariesContext dbContext) =>
            await dbContext.diaries.Where(diary => diary.GroupId == groupId).Select(diary => diary.ToDTO()).AsNoTracking().ToListAsync());
        group.MapGet("/{id}", [Authorize] async (int id, DiariesContext dbContext) => {
            var diary = await dbContext.diaries.FindAsync(id);
            if(diary is null) {
                Log.Error("Search of diary by id '{ID}' was unsuccessful", id);
                return Results.NotFound();
            }

            return Results.Ok(diary.ToDTO());
            }).WithName(getDiaryRoute);
        
        // mapping POST methods
        group.MapPost("/", [Authorize] async (CreateDiaryDTO createDiary, DiariesContext dbContext) => {
            try {
                Diary diary = createDiary.ToEntity();
                await dbContext.diaries.AddAsync(diary);
                await dbContext.SaveChangesAsync();
                Log.Information("Added diary with name: '{Name}' to group with id: '{Id}'", diary.Text, diary.GroupId);

                return Results.CreatedAtRoute(getDiaryRoute, new {id = diary.Id}, diary.ToDTO());
            } catch (Exception Ex) {
                Log.Fatal("Adding Diary was failed. Creating diary data: " +
                "{@creatingDiary} Exception text: {Exception}", createDiary, Ex);
                return Results.Problem("Exception message: " + Ex);
            }
        });

        /* mapping PUT methods
        Updating for diaries shouldn't be possible
        
        group.MapPut("/{id}", (int id) => {
            return Results.BadRequest("You can't change diaries");
        });

        /* mapping DELETE methods
        Deleting diaries shouldn't be possible
        group.MapDelete("/{id}", (int id) => {
            return Results.("You can't delete diaries");
        }); */

        return group;
    }
}
