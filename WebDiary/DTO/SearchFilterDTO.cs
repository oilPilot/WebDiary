namespace WebDiary.DTO;

public class SearchFilterDTO
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Mood { get; set; }
    public List<string>? Tags { get; set; }
    public string? Content { get; set; }
}