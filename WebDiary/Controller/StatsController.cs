using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using WebDiary.DTO;
namespace WebDiary.Controllers;

[Route("stats")]
[ApiController]
public class StatsController(IStatsService statsService) : ControllerBase
{

    [HttpGet("fordates")]
    public async Task<IActionResult> GetForDates(int userId, DateOnly startDate, DateOnly endDate)
    {
        StatsRequestDTO requestedDates = new StatsRequestDTO
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var stats = await statsService.GetStatsForPeriod(userId, requestedDates);
        // now i should to sort it by date
        stats.Sort((a, b) => a.Date.CompareTo(b.Date));

        return Ok(stats);
    }
    
    [HttpGet("adminNewUsers")]
    public async Task<IActionResult> GetNewUsers(DateOnly fromPeriod) =>
        Ok(await statsService.GetNewUsersCountAsync(ToDateTime(fromPeriod)));

    [HttpGet("adminNewEntries")]
    public async Task<IActionResult> GetNewEntries(DateOnly fromPeriod) =>
        Ok(await statsService.GetNewEntriesCountAsync(ToDateTime(fromPeriod)));

    [HttpGet("adminActiveUsers")]
    public async Task<IActionResult> GetActiveUsers(DateOnly fromPeriod) =>
        Ok(await statsService.CountActiveUsersInPeriod(ToDateTime(fromPeriod)));

    [HttpGet("adminInactiveUsers")]
    public async Task<IActionResult> GetInactiveUsers() => Ok(await statsService.GetNewInactiveUsersAsync());
    
    [HttpGet("admin30DayStatistics")]
    public async Task<IActionResult> Get30DaysStats() => Ok(await statsService.NewEntriesIn30DaysForChart());

    private DateTime ToDateTime(DateOnly fromPeriod) => fromPeriod.ToDateTime(TimeOnly.MinValue);

}
