namespace WebDiary.Frontend.Models;


public class ChatMessageDto
    {
    public string RoomKey { get; set; } = "";
    public string RoomType { get; set; } = "";
    public string Sender { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime SentAtUtc { get; set; }
}
public class ServerRoomJoinResult
{
    public string RoomKey { get; set; } = "";
    public ServerRoomSummary Room { get; set; } = new();
    public List<ChatMessageDto> Messages { get; set; } = [];
}
public class LocalMessageRecord
{
    public string Sender { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime SentAtUtc { get; set; }
}
public class ChatMessageViewModel
{
    public string Sender { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime SentAtUtc { get; set; }
    public bool IsFromCurrentUser { get; set; }
}