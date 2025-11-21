using System.ComponentModel.DataAnnotations;

namespace WebDiary.DTO;

// DTO for daily statistics
public record class StatsDayDTO
{
    public DateOnly Date { get; set; }
    public int EntriesCount { get; set; }
    public int SymbolsCount { get; set; }
    public double AvgSymbolsPerEntry { get; set; }
    public int MoodPointChange { get; set; }
}
// DTO for requesting statistics within a date range

public record class StatsRequestDTO
{
    [Required]
    public DateOnly StartDate { get; set; }
    [Required]
    public DateOnly EndDate { get; set; }
}
