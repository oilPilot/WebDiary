
namespace WebDiary.Entities;

public class LogRecord
{
    public int Id { get; set; }
    public required string message { get; set; }
    public required string message_template { get; set; }
    public int level { get; set; }
    public DateTime timestamp { get; set; }
    public string? exception { get; set; }
    public string? log_event { get; set; }
}
