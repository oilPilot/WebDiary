using System;
using Microsoft.EntityFrameworkCore;

namespace WebDiary.Data;

public static class ExtensionDb
{
    public static async Task MigrateDbAsync(this WebApplication app) {
        var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetService<DiariesContext>();
        await db!.Database.MigrateAsync();
    }
}
