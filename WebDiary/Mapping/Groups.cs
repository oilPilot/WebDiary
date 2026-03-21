using System;
using Microsoft.AspNetCore.Identity;
using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Mapping;

public static class Groups
{
    public static DiaryGroup toEntity(this CreateGroupDTO createGroup) {
        var hasher = new PasswordHasher<DiaryGroup>();
        return new DiaryGroup() {
            Name = createGroup.Name,
            UserId = createGroup.UserId,
            PinCode = !string.IsNullOrEmpty(createGroup.PinCode) ? hasher.HashPassword(new DiaryGroup() {Name = ""}, createGroup.PinCode) : "",
            IsArchived = false
        };
    }
    public static DiaryGroup toEntity(this UpdateGroupDTO newGroup, int id, int userId) {
        var hasher = new PasswordHasher<DiaryGroup>();
        return new DiaryGroup() {
            Id = id,
            Name = newGroup.Name,
            UserId = userId,
            PinCode = !string.IsNullOrEmpty(newGroup.PinCode) ? hasher.HashPassword(new DiaryGroup() {Name = ""}, newGroup.PinCode) : "",
            IsArchived = newGroup.IsArchived
        };
    }
    public static GroupDTO toDTO(this DiaryGroup group) {
        return new GroupDTO {
            Id = group.Id,
            Name = group.Name,
            OwnerId = group.UserId,
            OwnerUserName = group.Owner?.UserName ?? "Unknown",
            PinCode = !string.IsNullOrEmpty(group.PinCode) ? "Exists" : "",
            IsArchived = group.IsArchived,
            CreatedAtUtc = group.CreatedAtUtc,
            UpdatedAtUtc = group.UpdatedAtUtc
        };
    }
}
