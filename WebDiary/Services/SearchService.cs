using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Entities;
using WebDiary.Mapping;

public class SearchService : ISearchService
{
    private readonly DiariesContext _dbContext;

    public SearchService(DiariesContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<DiaryDTO>> SearchEntriesAsync(int userId, SearchFilterDTO filters)
    {
        try
        {
            // Get all group IDs the user has access to (owned + shared)
            var ownedGroupIds = _dbContext.diaryGroups
                .Where(group => group.UserId == userId)
                .Select(group => group.Id);

            var sharedGroupIds = _dbContext.groupPermissions
                .Where(p => p.UserId == userId)
                .Select(p => p.GroupId);

            var accessibleGroupIds = ownedGroupIds.Union(sharedGroupIds).ToList();

            // Start with all accessible diaries
            IQueryable<Diary> query = _dbContext.diaries
                .Where(d => accessibleGroupIds.Contains(d.GroupId))
                .Include(d => d.Owner);

            // Apply date filter
            if (filters.StartDate.HasValue)
            {
                query = query.Where(d => d.Date >= filters.StartDate);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(d => d.Date <= filters.EndDate);
            }

            // Apply mood filter
            if (!string.IsNullOrWhiteSpace(filters.Mood))
            {
                query = query.Where(d => d.mood == filters.Mood);
            }

            // Apply tags filter (if any tag provided, entry must have at least one matching tag)
            if (filters.Tags != null && filters.Tags.Any())
            {
                query = query.Where(d => d.Tags.Any(t => filters.Tags.Contains(t)));
            }

            // Apply content/text filter (full-text search)
            if (!string.IsNullOrWhiteSpace(filters.Content))
            {
                var searchTerm = filters.Content.ToLower();
                query = query.Where(d => 
                    d.BaseText.ToLower().Contains(searchTerm) || 
                    d.Text.ToLower().Contains(searchTerm)
                );
            }

            // Execute query and map to DTOs
            var results = await query
                .AsNoTracking()
                .OrderByDescending(d => d.Date)
                .ThenByDescending(d => d.Time)
                .ToListAsync();

            return results.Select(d => d.ToDTO()).ToList();
        }
        catch (Exception ex)
        {
            Log.Error("Error searching entries for user {UserId}: {@Exception}", userId, ex);
            throw;
        }
    }
}

