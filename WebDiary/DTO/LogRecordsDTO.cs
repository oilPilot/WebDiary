
namespace WebDiary.DTO;

public record class LogRecordsDTO
{
    public required string level { get; set; }
    public int levelInt { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Message { get; set; }
    public string Exception { get; set; } = "";
    public string Properties { get; set; } = "";
}
