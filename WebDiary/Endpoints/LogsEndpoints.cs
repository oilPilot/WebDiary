using System;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Mapping;

namespace WebDiary.Endpoints;

public static class LogsEndpoints
{
    const string getUserRoute = "LogsEndpoint";

    public static RouteGroupBuilder AddLogsEndpoint(this WebApplication app) {
        var group = app.MapGroup("logs");

        // mapping GET methods
        group.MapGet("/{count}", async (int count, DiariesContext dbContext) => {
            Log.Information("Getting all logs");
            return await dbContext.logs.OrderByDescending(log => log.timestamp)
                .Select(logs => logs.toDTO()).Take(count).AsNoTracking().ToListAsync();
            });
        
        // Serilog Sink gives us posting automatically
        // Update is not needed for logging

        // mapping DELETE methods
        group.MapDelete("/", async (DiariesContext dbContext) => {
            await dbContext.logs.ExecuteDeleteAsync();
            Log.Information("Cleared logs");
            return Results.NoContent();
        });

        return group;
    }

}
