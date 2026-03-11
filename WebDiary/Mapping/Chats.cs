using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Mapping;

public static class Chats
{
    public static ChatMessageDto ToMessageDto(this ChatMessage message, string roomKey, ChatRoomType roomType)
    {
        return new ChatMessageDto(
            roomKey,
            roomType.ToString().ToLowerInvariant(),
            message.SenderUserName,
            message.Content,
            message.SentAtUtc);
    }

    public static ChatRoomSummaryDto ToSummaryDto(this ChatRoom chatRoom)
    {
        return new ChatRoomSummaryDto(
            chatRoom.Id,
            chatRoom.Name,
            chatRoom.Type.ToString().ToLowerInvariant());
    }
}
