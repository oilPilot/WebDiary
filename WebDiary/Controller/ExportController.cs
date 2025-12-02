using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebDiary.Data;
using WebDiary.Entities;
using WebDiary.Helpers;

[Route("export")]
[ApiController]
public class ExportController(IExportService exportService) : ControllerBase
{
    [HttpGet("all/{userId:int}")]
    [Authorize]
    public IActionResult ExportAllDiaryGroup(int? userId = null)
    {
        if (userId != null)
        {
            var pdfBytes = exportService.GetExportForEveryDiary(userId);
            return File(pdfBytes, "application/pdf", "MyDiary.pdf");
        }
        else throw new Exception("ExportDiary: both group and user id was null");
        
    }
    
    [HttpGet("certain/{groupId:int}")]
    public IActionResult ExportCertainDiaryGroup(int? groupId = null)
    {
        if (groupId != null)
        {
            var pdfBytes = exportService.GetExportForCertainGroup(groupId);
            return File(pdfBytes, "application/pdf", "MyDiary.pdf");
        }
        else throw new Exception("ExportDiary: both group and user id was null");
        
    }
}
