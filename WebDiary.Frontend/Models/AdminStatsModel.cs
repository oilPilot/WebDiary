
namespace WebDiary.Frontend.Models;

public class AdminStatsModel {
    public DateOnly DateOfData { get; set; }
    public int EntriesCount { get; set; }
    public int SymbolsCount { get; set; }
}