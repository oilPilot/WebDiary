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

}
