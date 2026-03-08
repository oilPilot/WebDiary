using Microsoft.EntityFrameworkCore;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Entities;
using WebDiary.Helpers;
using WebDiary.Mapping;

public class ExportService : IExportService
{
    private readonly DiariesContext dbContext;

    public ExportService(DiariesContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public byte[] GetExportForEveryDiary(int userId)
    {
        List<Diary> diaries;
        var groups = dbContext.diaryGroups.Where(group => group.UserId == userId);
        diaries = dbContext.diaries.Where(diary => groups.Any(group => group.Id == diary.GroupId)).ToList();
        return new DiaryPdfExporter().Export(diaries);
    }
    public byte[] GetExportForCertainGroup(int groupId)
    {
        List<Diary> entries;
        entries = dbContext.diaries.Where(diary => diary.GroupId == groupId).ToList();
        var groupName = dbContext.diaryGroups
            .Where(group => group.Id == groupId)
            .Select(group => group.Name)
            .FirstOrDefault() ?? "MyDiary";
        return new DiaryPdfExporter().Export(entries, groupName);
    }
}
