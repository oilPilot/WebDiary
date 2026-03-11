namespace WebDiary.DTO;

public sealed record ChatRoomSummaryDto(int Id, string Name, string Type);

public sealed record ChatMessageDto(
    string RoomKey,
    string RoomType,
    string Sender,
    string Content,
    DateTime SentAtUtc
);

public sealed record ServerRoomJoinDto(
    string RoomKey,
    ChatRoomSummaryDto Room,
    List<ChatMessageDto> Messages
);
