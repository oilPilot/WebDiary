using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebDiary.Data;
using WebDiary.Entities;
using WebDiary.DTO;
using WebDiary.Mapping;

namespace WebDiary.Hubs;

[Authorize]
public class ChatHub(DiariesContext dbContext) : Hub
{
    private const int MaxRetainedMessages = 150;
    private static readonly TimeSpan MessageLifetime = TimeSpan.FromHours(72);
    private const string LocalRoomPrefix = "local-room:";
    private const string ServerRoomPrefix = "server-room:";

    public async Task<IReadOnlyList<ChatRoomSummaryDto>> GetServerChatRoomsAsync()
    {
        await EnsureDefaultServerRoomsAsync();

        return await dbContext.chatRooms
            .OrderBy(room => room.Type)
            .ThenBy(room => room.Name)
            .Select(room => room.ToSummaryDto())
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<ChatRoomSummaryDto> CreateServerChatRoomAsync(string roomName, string roomType)
    {
        return Task.FromException<ChatRoomSummaryDto>(new HubException("Server chatroom creation is disabled."));
    }

    public async Task<ServerRoomJoinDto> JoinServerRoomAsync(int roomId)
    {
        var room = await dbContext.chatRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(chatRoom => chatRoom.Id == roomId);
        if (room is null)
        {
            throw new HubException("Chatroom not found.");
        }

        await ApplyRetentionAsync(roomId);

        var groupKey = BuildServerRoomKey(roomId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupKey);

        var latestMessages = await dbContext.chatMessages
            .Where(message => message.ChatRoomId == roomId)
            .OrderByDescending(message => message.SentAtUtc)
            .ThenByDescending(message => message.Id)
            .Take(MaxRetainedMessages)
            .AsNoTracking()
            .ToListAsync();

        var orderedMessages = latestMessages
            .OrderBy(message => message.SentAtUtc)
            .ThenBy(message => message.Id)
            .Select(message => message.ToMessageDto(groupKey, room.Type))
            .ToList();
        // Aren't we are already done this in latestMessages?? --- IGNORE ---

        return new ServerRoomJoinDto(
            groupKey,
            room.ToSummaryDto(),
            orderedMessages);
    }

    public async Task<string> JoinLocalRoomAsync(string joinToken)
    {
        var roomKey = BuildLocalRoomKey(joinToken);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomKey);
        return roomKey;
    }

    public Task LeaveRoomAsync(string roomKey)
    {
        if (string.IsNullOrWhiteSpace(roomKey))
        {
            return Task.CompletedTask;
        }

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, roomKey.Trim());
    }

    public async Task SendServerMessageAsync(int roomId, string messageText)
    {
        var room = await dbContext.chatRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(chatRoom => chatRoom.Id == roomId);
        if (room is null)
        {
            throw new HubException("Chatroom not found.");
        }

        var content = NormalizeMessageText(messageText);
        var (userId, userName) = GetUserContext();
        var message = new ChatMessage
        {
            ChatRoomId = room.Id,
            SenderUserId = userId,
            SenderUserName = userName,
            Content = content,
            SentAtUtc = DateTime.UtcNow
        };

        await dbContext.chatMessages.AddAsync(message);
        await dbContext.SaveChangesAsync();

        await ApplyRetentionAsync(roomId);

        var groupKey = BuildServerRoomKey(roomId);
        await Clients.Group(groupKey).SendAsync("ReceiveMessage", message.ToMessageDto(groupKey, room.Type));
    }

    public async Task SendLocalMessageAsync(string roomKey, string messageText)
    {
        if (string.IsNullOrWhiteSpace(roomKey))
        {
            throw new HubException("Invalid local chatroom key.");
        }

        var normalizedRoomKey = roomKey.Trim();
        if (!normalizedRoomKey.StartsWith(LocalRoomPrefix, StringComparison.Ordinal))
        {
            throw new HubException("Invalid local chatroom key.");
        }

        var (_, userName) = GetUserContext();
        var content = NormalizeMessageText(messageText);
        var message = new ChatMessageDto(
            normalizedRoomKey,
            "local",
            userName,
            content,
            DateTime.UtcNow);

        await Clients.Group(normalizedRoomKey).SendAsync("ReceiveMessage", message);
    }

    // HELPERS

    private async Task EnsureDefaultServerRoomsAsync()
    {
        if (await dbContext.chatRooms.AnyAsync())
        {
            return;
        }

        await dbContext.chatRooms.AddRangeAsync(
            new ChatRoom
            {
                Name = "Public Lobby",
                Type = ChatRoomType.Public,
                CreatedAtUtc = DateTime.UtcNow
            },
            new ChatRoom
            {
                Name = "Ask Admin",
                Type = ChatRoomType.Admin,
                CreatedAtUtc = DateTime.UtcNow
            });

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
        }
    }

    private async Task ApplyRetentionAsync(int roomId)
    {
        var expiration = DateTime.UtcNow.Subtract(MessageLifetime);
        await dbContext.chatMessages
            .Where(message => message.ChatRoomId == roomId && message.SentAtUtc < expiration)
            .ExecuteDeleteAsync();

        var idsOutsideLimit = await dbContext.chatMessages
            .Where(message => message.ChatRoomId == roomId)
            .OrderByDescending(message => message.SentAtUtc)
            .ThenByDescending(message => message.Id)
            .Skip(MaxRetainedMessages)
            .Select(message => message.Id)
            .ToListAsync();

        if (idsOutsideLimit.Count == 0)
        {
            return;
        }

        await dbContext.chatMessages
            .Where(message => idsOutsideLimit.Contains(message.Id))
            .ExecuteDeleteAsync();
    }

    private static string BuildServerRoomKey(int roomId)
    {
        return $"{ServerRoomPrefix}{roomId}";
    }

    private static string BuildLocalRoomKey(string joinToken)
    {
        if (string.IsNullOrWhiteSpace(joinToken))
        {
            throw new HubException("Join token is required for a local chatroom.");
        }

        var normalized = joinToken.Trim();
        if (normalized.Length is < 6 or > 256)
        {
            throw new HubException("Local chatroom join token must be between 6 and 256 characters.");
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return $"{LocalRoomPrefix}{Convert.ToHexString(bytes)}";
    }

    private static string NormalizeRoomName(string roomName)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            throw new HubException("Room name is required.");
        }

        var normalized = roomName.Trim();
        if (normalized.Length is < 2 or > 100)
        {
            throw new HubException("Room name length must be between 2 and 100 characters.");
        }

        return normalized;
    }

    private static ChatRoomType ParseServerRoomType(string roomType)
    {
        if (!Enum.TryParse<ChatRoomType>(roomType, true, out var parsedRoomType))
        {
            throw new HubException("Unsupported room type.");
        }

        if (parsedRoomType is not ChatRoomType.Public and not ChatRoomType.Admin)
        {
            throw new HubException("Unsupported room type.");
        }

        return parsedRoomType;
    }

    private static string NormalizeMessageText(string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
        {
            throw new HubException("Message must not be empty.");
        }

        var normalized = messageText.Trim();
        if (normalized.Length > 2_000)
        {
            throw new HubException("Message is too long.");
        }

        return normalized;
    }

    private (int UserId, string UserName) GetUserContext()
    {
        var principal = Context.User;
        var userIdClaim = principal?.FindFirst("userId")?.Value
            ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new HubException("Unauthorized.");
        }

        var userName = principal?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = $"User #{userId}";
        }

        return (userId, userName);
    }
}
