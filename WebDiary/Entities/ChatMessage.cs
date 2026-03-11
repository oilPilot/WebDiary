namespace WebDiary.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public int SenderUserId { get; set; }
    public required string SenderUserName { get; set; }
    public required string Content { get; set; }
    public DateTime SentAtUtc { get; set; }
}
