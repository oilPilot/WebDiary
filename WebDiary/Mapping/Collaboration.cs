using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Mapping;

/// <summary>
/// Mapping functions for collaboration-related entities (GroupPermission, EntryReference)
/// </summary>
public static class Collaboration
{
    /// <summary>
    /// Convert GroupPermission entity to DTO
    /// </summary>
    public static GroupPermissionDTO toDTO(this GroupPermission permission)
    {
        return new GroupPermissionDTO
        {
            Id = permission.Id,
            GroupId = permission.GroupId,
            UserId = permission.UserId,
            UserName = permission.User?.UserName ?? "Unknown",
            Role = permission.Role,
            GrantedByUserName = permission.GrantedByUser?.UserName ?? "Unknown",
            CreatedAtUtc = permission.CreatedAtUtc,
            UpdatedAtUtc = permission.UpdatedAtUtc
        };
    }

    /// <summary>
    /// Convert EntryReference entity to DTO
    /// </summary>
    public static EntryReferenceDTO toDTO(this EntryReference reference)
    {
        return new EntryReferenceDTO
        {
            Id = reference.Id,
            SourceEntryId = reference.SourceEntryId,
            ReferencedEntryId = reference.ReferencedEntryId,
            ReferencedEntryOwner = reference.ReferencedEntry?.Owner?.UserName ?? "Unknown",
            ExcerptText = reference.ExcerptText,
            ExcerptStartIndex = reference.ExcerptStartIndex,
            ExcerptEndIndex = reference.ExcerptEndIndex,
            CreatedAtUtc = reference.CreatedAtUtc
        };
    }

    /// <summary>
    /// Convert CreateEntryReferenceDTO to EntryReference entity
    /// </summary>
    public static EntryReference toEntity(this CreateEntryReferenceDTO dto)
    {
        return new EntryReference
        {
            SourceEntryId = dto.SourceEntryId,
            ReferencedEntryId = dto.ReferencedEntryId,
            ExcerptText = dto.ExcerptText,
            ExcerptStartIndex = dto.ExcerptStartIndex,
            ExcerptEndIndex = dto.ExcerptEndIndex,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
