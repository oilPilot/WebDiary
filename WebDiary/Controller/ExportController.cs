using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebDiary.Data;
using WebDiary.Entities;
using WebDiary.Helpers;

[Route("export")]
[ApiController]
[Authorize]
public class ExportController(IExportService exportService, DiariesContext dbContext) : ControllerBase
{
    [HttpGet("all/{userId:int}")]
    public IActionResult ExportAllDiaryGroup(int userId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (!User.IsInRole("Admin") && currentUserId.Value != userId)
        {
            return Forbid();
        }

        var pdfBytes = exportService.GetExportForEveryDiary(userId);
        return File(pdfBytes, "application/pdf", "MyDiary.pdf");
    }
    
    [HttpGet("certain/{groupId:int}")]
    public async Task<IActionResult> ExportCertainDiaryGroup(int groupId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (!User.IsInRole("Admin"))
        {
            var isOwner = await dbContext.diaryGroups
                .AnyAsync(group => group.Id == groupId && group.UserId == currentUserId.Value);
            if (!isOwner)
            {
                return Forbid();
            }
        }

        var pdfBytes = exportService.GetExportForCertainGroup(groupId);
        return File(pdfBytes, "application/pdf", "MyDiary.pdf");
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
