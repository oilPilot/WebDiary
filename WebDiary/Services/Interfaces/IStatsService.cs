using WebDiary.DTO;

public interface IStatsService
{
    Task<List<StatsDayDTO>> GetStatsForPeriod(int userId, StatsRequestDTO DatePeriod);
}
