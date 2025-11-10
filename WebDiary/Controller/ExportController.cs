using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebDiary.Data;
using WebDiary.Entities;

[Route("export")]
[ApiController]
public class ExportController(DiariesContext dbContext) : ControllerBase
{
    [HttpGet("all/{userId:int}")]
    [Authorize]
    public IActionResult ExportAllDiaryGroup(int? userId = null)
    {
        List<Diary> diaries;

        if (userId != null)
        {
            var groups = dbContext.diaryGroups.Where(group => group.UserId == userId);
            diaries = dbContext.diaries.Where(diary => groups.Any(group => group.Id == diary.GroupId)).ToList();

            var pdfBytes = new DiaryPdfExporter().Export(diaries);
            return File(pdfBytes, "application/pdf", "MyDiary.pdf");
        }
        else throw new Exception("ExportDiary: both group and user id was null");
        
    }
    
    [HttpGet("certain/{groupId:int}")]
    public IActionResult ExportCertainDiaryGroup(int? groupId = null)
    {
        List<Diary> entries;

        if (groupId != null)
        {
            entries = dbContext.diaries.Where(diary => diary.GroupId == groupId).ToList();
            var pdfBytes = new DiaryPdfExporter().Export(entries, dbContext.diaryGroups.Where(group => group.Id == groupId).First().Name);
            return File(pdfBytes, "application/pdf", "MyDiary.pdf");
        }
        else throw new Exception("ExportDiary: both group and user id was null");
        
    }
}
