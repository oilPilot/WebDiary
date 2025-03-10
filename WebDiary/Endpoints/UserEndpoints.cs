using System;
using Microsoft.EntityFrameworkCore;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Mapping;

namespace WebDiary.Endpoints;

public static class UserEndpoints
{
    const string getUserRoute = "UserEndpoint";

    public static RouteGroupBuilder AddUsersEndpoint(this WebApplication app) {
        var group = app.MapGroup("users");

        // mapping GET methods
        group.MapGet("/", async (DiariesContext dbContext) => await dbContext.users.Select(user => user.toDTO()).AsNoTracking().ToListAsync());
        group.MapGet("/{id}", async (int id, DiariesContext dbContext) => {
            var user = await dbContext.users.FindAsync(id);
            if(user is null) {
                return Results.NotFound();
            }

            return Results.Ok(user.toDTO());
        }).WithName(getUserRoute);
        
        // mapping POST methods
        group.MapPost("/", async (CreateUserDTO newUser, DiariesContext dbContext) => {
            var user = newUser.toEntity();

            await dbContext.users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(getUserRoute, new {id = user.Id}, user.toDTO());
        });

        // mapping PUT methods
        group.MapPut("/{id}", async (int id, UpdateUserDTO newUser, DiariesContext dbContext) => {
            var currentUser = await dbContext.users.FindAsync(id);
            if(currentUser is null) {
                return Results.NotFound();
            }

            var user = newUser.toEntity(currentUser);
            dbContext.users.Entry(currentUser).CurrentValues.SetValues(user);
            await dbContext.SaveChangesAsync();
            
            return Results.NoContent();
        });

        // mapping DELETE methods
        group.MapDelete("/{id}", async (int id, DiariesContext dbContext) => {
            await dbContext.users.Where(user => user.Id == id).ExecuteDeleteAsync();
            await dbContext.diaries.Where(diary => dbContext.diaryGroups.Where(group => group.UserId == id).Where(group => group.Id == diary.GroupId).ToList().Contains(diary.Group)).ExecuteDeleteAsync();
            await dbContext.diaryGroups.Where(group => group.UserId == id).ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }

}
