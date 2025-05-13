using System;
using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Mapping;

public static class Groups
{
    public static DiaryGroup toEntity(this CreateGroupDTO createGroup) {
        return new DiaryGroup() {
            Name = createGroup.Name,
            UserId = createGroup.UserId
        };
    }
    public static DiaryGroup toEntity(this UpdateGroupDTO newGroup, int id, int userId) {
        return new DiaryGroup() {
            Id = id,
            Name = newGroup.Name,
            UserId = userId
        };
    }
    public static GroupDTO toDTO(this DiaryGroup group) {
        return new GroupDTO {
            Id = group.Id,
            Name = group.Name
        };
    }
}
