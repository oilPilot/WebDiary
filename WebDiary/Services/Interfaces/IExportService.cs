public interface IExportService
{
    byte[] GetExportForEveryDiary(int? userId);
    byte[] GetExportForCertainGroup(int? groupId);
}
