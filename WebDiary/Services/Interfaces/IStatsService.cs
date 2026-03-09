using WebDiary.DTO;
using WebDiary.Entities;
using WebDiary.Model;

public interface IStatsService
{
    Task<List<StatsDayDTO>> GetStatsForPeriod(int userId, StatsRequestDTO DatePeriod);
    Task<int> GetNewUsersCountAsync(DateTime fromPeriod);
    Task<int> GetNewEntriesCountAsync(DateTime fromPeriod);
    Task<int> CountActiveUsersInPeriod(DateTime fromPeriod); // Typically Day or Week
    Task<List<UserDTO>> GetNewInactiveUsersAsync(int daysInactive = 30);
    Task<List<AdminStatsModel>> NewEntriesIn30DaysForChart();
    // currently used by admin statistics page - most active users over all time or a given period
    Task<List<UserActivityDTO>> GetMostActiveUsersAsync(int limit);
}
