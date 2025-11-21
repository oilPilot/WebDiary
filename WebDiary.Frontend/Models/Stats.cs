namespace WebDiary.Frontend.Models;

public record class StatsDay
{
    public DateOnly Date { get; set; }
    public int EntriesCount { get; set; }
    public int SymbolsCount { get; set; }
    public double AvgSymbolsPerEntry { get; set; }
    public int MoodPointChange { get; set; }
}

