using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Entities;
using WebDiary.Services;

namespace WebDiary.Endpoints;

public static class SearchEndpoints
{
    public static RouteGroupBuilder AddSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("search").RequireAuthorization();

        // GET /search/entries - Filter/search all accessible entries without grouping
        group.MapPost("/entries", async (
            SearchFilterDTO filters,
            ClaimsPrincipal principal,
            ISearchService searchService) =>
        {
            try
            {
                var userId = GetCurrentUserId(principal);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var results = await searchService.SearchEntriesAsync(userId.Value, filters);
                
                Log.Information("Returned {Count} search results for user {UserId} with filters: {@Filters}", 
                    results.Count, userId, filters);
                
                return Results.Ok(results);
            }
            catch (Exception ex)
            {
                Log.Error("Error searching entries: {@Exception}", ex);
                return Results.Problem("Error searching entries");
            }
        }).WithName("SearchEntries");

        // Get unique moods for filtering
        group.MapGet("/moods", async (
            ClaimsPrincipal principal,
            DiariesContext dbContext) =>
        {
            try
            {
                var userId = GetCurrentUserId(principal);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var ownedGroupIds = dbContext.diaryGroups
                    .Where(group => group.UserId == userId)
                    .Select(group => group.Id);

                var sharedGroupIds = dbContext.groupPermissions
                    .Where(p => p.UserId == userId)
                    .Select(p => p.GroupId);

                var accessibleGroupIds = ownedGroupIds.Union(sharedGroupIds);

                var moods = await dbContext.diaries
                    .Where(d => accessibleGroupIds.Contains(d.GroupId))
                    .Select(d => d.mood)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync();

                return Results.Ok(moods);
            }
            catch (Exception ex)
            {
                Log.Error("Error retrieving moods: {@Exception}", ex);
                return Results.Problem("Error retrieving moods");
            }
        }).WithName("GetMoods");

        // Get unique tags for filtering
        group.MapGet("/tags", async (
            ClaimsPrincipal principal,
            DiariesContext dbContext) =>
        {
            try
            {
                var userId = GetCurrentUserId(principal);
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var ownedGroupIds = dbContext.diaryGroups
                    .Where(group => group.UserId == userId)
                    .Select(group => group.Id);

                var sharedGroupIds = dbContext.groupPermissions
                    .Where(p => p.UserId == userId)
                    .Select(p => p.GroupId);

                var accessibleGroupIds = ownedGroupIds.Union(sharedGroupIds);

                var tags = await dbContext.diaries
                    .Where(d => accessibleGroupIds.Contains(d.GroupId))
                    .SelectMany(d => d.Tags)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();

                return Results.Ok(tags);
            }
            catch (Exception ex)
            {
                Log.Error("Error retrieving tags: {@Exception}", ex);
                return Results.Problem("Error retrieving tags");
            }
        }).WithName("GetTags");

        return group;
    }

    private static int? GetCurrentUserId(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim is not null && int.TryParse(userIdClaim.Value, out var id) ? id : null;
    }
}
