using WebDiary.DTO;
using WebDiary.Entities;

public interface IStatsService
{
    Task<List<StatsDayDTO>> GetStatsForPeriod(int userId, StatsRequestDTO DatePeriod);
    Task<int> GetNewUsersCountAsync(DateTime fromPeriod);
    Task<int> GetNewEntriesCountAsync(DateTime fromPeriod);
    Task<int> CountActiveUsersInPeriod(DateTime fromPeriod); // Typically Day or Week
    Task<List<User>> GetNewInactiveUsersAsync(int daysInactive = 30);
    Task<List<(DateOnly dateOfData, int entriesCount, int symbolsCount)>> NewEntriesIn30DaysForChart();
    // Task<IReadOnlyList<UserActivityDto>> GetMostActiveUsers(int limit) - Maybe one day... One day i will implement it
}
