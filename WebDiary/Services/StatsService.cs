using Microsoft.EntityFrameworkCore;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Entities;
using WebDiary.Helpers;
using WebDiary.Mapping;
using WebDiary.Model;

public class StatsService : IStatsService
{
    private readonly DiariesContext dbContext;

    public StatsService(DiariesContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<StatsDayDTO>> GetStatsForPeriod(int userId, StatsRequestDTO DatePeriod)
    {
//        string cacheKey = $"stats_{userId}_{year}_{month}";
//        if (_cache.TryGetValue(cacheKey, out PeriodStatsDto cached))
//            return cached;
        var groupIds = await dbContext.diaryGroups.Where(group => group.UserId == userId).Select(group => group.Id).ToListAsync();
        var diaries = await dbContext.diaries.Where(diary => groupIds.Contains(diary.GroupId)).AsNoTracking().ToListAsync();

        var filtered = diaries.Where(d => d.Date >= DatePeriod.StartDate && d.Date <= DatePeriod.EndDate).ToList();

        var stats = CalculateStats(filtered);

//        _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));
        return stats;
    }

    public async Task<int> GetNewUsersCountAsync(DateTime fromPeriod)
    {
        var users = await dbContext.users.Where(user => user.CreatedAtUTC >= fromPeriod).CountAsync();
        return users;
    }

    public async Task<int> GetNewEntriesCountAsync(DateTime fromPeriod)
    {
        var entries = await dbContext.diaries.Where(entry => entry.CreatedUtc >= fromPeriod).CountAsync();
        return entries;
    }

    public async Task<int> CountActiveUsersInPeriod(DateTime fromPeriod)
    {
        var users = await dbContext.users.Where(user => user.LastLoginAtUTC >= fromPeriod).CountAsync();
        return users;
    }

    public async Task<List<User>> GetNewInactiveUsersAsync(int daysInactive = 30)
    {
        var maxTime = DateTime.UtcNow.AddDays(-daysInactive);
        var users = await dbContext.users.Where(user => user.LastLoginAtUTC <= maxTime).ToListAsync();
        return users;
    }

    public async Task<List<AdminStatsModel>> NewEntriesIn30DaysForChart()
    {
        var returnList = new List<AdminStatsModel>();
        var startDate = new DateTime(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), TimeOnly.MinValue);
        var diaries = await dbContext.diaries.Where(entry => entry.CreatedUtc >= startDate).ToListAsync();

        for(DateTime date = startDate; date <= DateTime.UtcNow; date = date.AddDays(1)) {
            var todayDiaries = diaries.Where(entry => DateOnly.FromDateTime(entry.CreatedUtc)
                == DateOnly.FromDateTime(date)).ToList();
            var newValue = new AdminStatsModel {
                DateOfData = DateOnly.FromDateTime(date),
                EntriesCount = todayDiaries.Count,
                SymbolsCount = todayDiaries.Sum(d => d.BaseText.Length)
            };
            returnList.Add(newValue);
        }

        return returnList;
    }

    // Helpers

    private List<StatsDayDTO> CalculateStats(List<Diary> diaries)
    {
        List<StatsDayDTO> statsList = new List<StatsDayDTO>();

        foreach(var date in diaries.Select(d => d.Date).Distinct()) {
            statsList.Add(CalculateDailyStats(diaries.Where(d => d.Date == date).ToList()));
        }

        return statsList;
    }

    private StatsDayDTO CalculateDailyStats(List<Diary> diary)
    {
        int totalSymbols = diary.Sum(d => d.BaseText.Length);
        int moodPoint = diary.Sum(d => MoodHelper.GetMoodValue(d.mood));
        return new StatsDayDTO
        {
            Date = diary.First().Date,
            EntriesCount = diary.Count,
            SymbolsCount = totalSymbols,
            AvgSymbolsPerEntry = diary.Count == 0 ? 0 : totalSymbols / (double)diary.Count,
            MoodPointChange = moodPoint
        };
    }
}
