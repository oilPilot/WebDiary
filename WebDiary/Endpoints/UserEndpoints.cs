using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
        var group = app.MapGroup("users").RequireAuthorization();

        // mapping GET methods
        group.MapGet("/", [Authorize(Roles = "Admin")] async (DiariesContext dbContext) => {
            Log.Information("Getting all users");
            return await dbContext.users.Select(user => user.toDTO()).AsNoTracking().ToListAsync();
            });
        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal principal, DiariesContext dbContext) =>
        {
            if (!CanAccessUser(principal, id))
            {
                return Results.Forbid();
            }

            var user = await dbContext.users.FindAsync(id);
            if (user is null)
            {
                Log.Error("Search of user by id '{ID}' was unsuccessful", id);
                return Results.NotFound();
            }

            return Results.Ok(user.toDTO());
        }).WithName(getUserRoute);
        /* NO ANYMORE EMAILS
        group.MapGet("/byemail/{email}", async (string email, DiariesContext dbContext) => {
            var user = await dbContext.users.AsNoTracking().Where(user => user.Email == email).ToListAsync();
            if(user.FirstOrDefault() is null) {
                Log.Error("Search of user by email '{Email}' was unsuccessful", email);
                return Results.NotFound();
            }
            
            return Results.Ok(user.FirstOrDefault()!.toDTO());
        });
        */
        
        // mapping POST methods
        group.MapPost("/", async (CreateUserDTO newUser, DiariesContext dbContext) => {
            try {
                var userName = (newUser.UserName ?? string.Empty).Trim();
                var password = newUser.Password ?? string.Empty;
                if(string.IsNullOrWhiteSpace(userName) || userName.Length is < 2 or > 24)
                {
                    return Results.BadRequest("Username length must be between 2 and 24 characters.");
                }
                if(password.Length is < 8 or > 64)
                {
                    return Results.BadRequest("Password length must be between 8 and 64 characters.");
                }

                var isNameUsed = await dbContext.users
                    .AnyAsync(user => user.UserName.ToLower() == userName.ToLower());
                if(isNameUsed)
                {
                    return Results.Conflict("Username already exists.");
                }

                var payload = new CreateUserDTO
                {
                    UserName = userName,
                    Password = password,
                    Description = newUser.Description
                };
                var user = payload.toEntity();

                await dbContext.users.AddAsync(user);
                await dbContext.SaveChangesAsync();

                Log.Information("Created new user with id: '{ID}' name: '{Name}'", user.Id, user.UserName);

                return Results.CreatedAtRoute(getUserRoute, new {id = user.Id}, user.toDTO());
            } catch (Exception Ex) {
                Log.Fatal("Adding User was failed. New User data: " +
                "{@newUser} Exception text: {Exception}", newUser, Ex);
                return Results.Problem("Unexpected error while creating user.");
            }
        }).AllowAnonymous();

        // mapping PUT methods
        group.MapPut("/{id:int}", async (int id, UpdateUserDTO newUser, ClaimsPrincipal principal, DiariesContext dbContext) => {
            try {
                if (!CanAccessUser(principal, id))
                {
                    return Results.Forbid();
                }

                var currentUser = await dbContext.users.FindAsync(id);
                if(currentUser is null) {
                    Log.Error("Search of user by id '{ID}' upon updating was unsuccessful", id);
                    return Results.NotFound();
                }
                if(!string.IsNullOrWhiteSpace(newUser.UserName))
                {
                    var trimmedName = newUser.UserName.Trim();
                    if(trimmedName.Length is < 2 or > 24)
                    {
                        return Results.BadRequest("Username length must be between 2 and 24 characters.");
                    }

                    var alreadyTaken = await dbContext.users
                        .AnyAsync(user => user.Id != id && user.UserName.ToLower() == trimmedName.ToLower());
                    if(alreadyTaken)
                    {
                        return Results.Conflict("Username already exists.");
                    }
                }
                if(newUser.Password != null && newUser.Password.Length is < 8 or > 64)
                {
                    return Results.BadRequest("Password length must be between 8 and 64 characters.");
                }

                var user = newUser.toEntity(currentUser);
                dbContext.users.Entry(currentUser).CurrentValues.SetValues(user);
                await dbContext.SaveChangesAsync();
                
                return Results.NoContent();
            } catch (Exception Ex) {
                Log.Fatal("Updating User was failed. New User data: " +
                "{@newUser} Exception text: {Exception}", newUser, Ex);
                return Results.Problem("Unexpected error while updating user.");
            }
        });

        // mapping DELETE methods
        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal principal, DiariesContext dbContext) => {
            if (!CanAccessUser(principal, id))
            {
                return Results.Forbid();
            }

            var groupIds = dbContext.diaryGroups.Where(group => group.UserId == id).Select(group => group.Id);
            await dbContext.diaries.Where(diary => groupIds.Contains(diary.GroupId)).ExecuteDeleteAsync();
            await dbContext.diaryGroups.Where(group => group.UserId == id).ExecuteDeleteAsync();
            await dbContext.users.Where(user => user.Id == id).ExecuteDeleteAsync();
            
            Log.Information("Deleted user with id '{ID}' and it's groups with diaries", id);

            return Results.NoContent();
        });

        return group;
    }

    private static bool CanAccessUser(ClaimsPrincipal principal, int requiredUserId)
    {
        if (principal.IsInRole("Admin"))
        {
            return true;
        }

        var userIdClaim = principal.FindFirstValue("userId") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) && userId == requiredUserId;
    }

}
