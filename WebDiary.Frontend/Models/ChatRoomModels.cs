namespace WebDiary.Frontend.Models;

public class ServerRoomSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
}
public class LocalRoomRecord
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Secret { get; set; } = "";
    public string RoomKey { get; set; } = "";
}
public class SelectedRoom
{
    public string DisplayName { get; set; } = "";
    public string RoomType { get; set; } = "";
    public string RoomKey { get; set; } = "";
    public int? ServerRoomId { get; set; }
}
