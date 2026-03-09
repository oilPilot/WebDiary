using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using WebDiary.DTO;
namespace WebDiary.Controllers;

[Route("stats")]
[ApiController]
[Authorize]
public class StatsController(IStatsService statsService) : ControllerBase
{

    [HttpGet("fordates")]
    public async Task<IActionResult> GetForDates(DateOnly startDate, DateOnly endDate, int? userId = null)
    {
        var currentUserId = GetCurrentUserId();
        if(currentUserId is null)
        {
            return Unauthorized();
        }

        var requestedUserId = User.IsInRole("Admin") && userId.HasValue
            ? userId.Value
            : currentUserId.Value;

        StatsRequestDTO requestedDates = new StatsRequestDTO
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var stats = await statsService.GetStatsForPeriod(requestedUserId, requestedDates);
        // now i should to sort it by date
        stats.Sort((a, b) => a.Date.CompareTo(b.Date));

        return Ok(stats);
    }
    
    [HttpGet("adminNewUsers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetNewUsers(DateOnly fromPeriod) =>
        Ok(await statsService.GetNewUsersCountAsync(ToDateTime(fromPeriod)));

    [HttpGet("adminNewEntries")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetNewEntries(DateOnly fromPeriod) =>
        Ok(await statsService.GetNewEntriesCountAsync(ToDateTime(fromPeriod)));

    [HttpGet("adminActiveUsers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetActiveUsers(DateOnly fromPeriod) =>
        Ok(await statsService.CountActiveUsersInPeriod(ToDateTime(fromPeriod)));

    [HttpGet("adminInactiveUsers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetInactiveUsers() => Ok(await statsService.GetNewInactiveUsersAsync());
    
    [HttpGet("admin30DayStatistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Get30DaysStats() => Ok(await statsService.NewEntriesIn30DaysForChart());

    [HttpGet("adminMostActiveUsers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMostActiveUsers(int limit = 5) =>
        Ok(await statsService.GetMostActiveUsersAsync(limit));

    private DateTime ToDateTime(DateOnly fromPeriod) => fromPeriod.ToDateTime(TimeOnly.MinValue);

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

}
