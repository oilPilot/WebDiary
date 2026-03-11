namespace WebDiary.Entities;

public class ChatRoom
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ChatRoomType Type { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
}
