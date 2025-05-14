using System;
using Microsoft.EntityFrameworkCore;
using Serilog;
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
                Log.Error("Search of user by id '{ID}' was unsuccessful", id);
            if(user is null) {
                return Results.NotFound();
            }

            return Results.Ok(user.toDTO());
        }).WithName(getUserRoute);
        group.MapGet("/byemail/{email}", async (string email, DiariesContext dbContext) => {
            var user = await dbContext.users.AsNoTracking().Where(user => user.Email == email).ToListAsync();
            if(user.FirstOrDefault() is null) {
                Log.Error("Search of user by email '{Email}' was unsuccessful", email);
                return Results.NotFound();
            }
            
            return Results.Ok(user.FirstOrDefault()!.toDTO());
        });
        
        // mapping POST methods
        group.MapPost("/", async (CreateUserDTO newUser, DiariesContext dbContext) => {
            var user = newUser.toEntity();

            await dbContext.users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            Log.Information("Created new user with id: '{ID}' name: '{Name}'", user.Id, user.UserName);

            return Results.CreatedAtRoute(getUserRoute, new {id = user.Id}, user.toDTO());
        });

        // mapping PUT methods
        group.MapPut("/{id}", async (int id, UpdateUserDTO newUser, DiariesContext dbContext) => {
            var currentUser = await dbContext.users.FindAsync(id);
            if(currentUser is null) {
                Log.Error("Search of user by id '{ID}' upon updating was unsuccessful", id);
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
            await dbContext.diaries.Where(diary => dbContext.diaryGroups.Where(group => group.UserId == id)
                                    .Where(group => group.Id == diary.GroupId).ToList().Contains(diary.Group!)).ExecuteDeleteAsync();
            await dbContext.diaryGroups.Where(group => group.UserId == id).ExecuteDeleteAsync();
            
            Log.Information("Deleted user with id '{ID}' and it's groups with diaries", id);

            return Results.NoContent();
        });

        return group;
    }

}
