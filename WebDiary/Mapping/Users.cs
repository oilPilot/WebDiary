using System;
using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Mapping;

public static class Users
{
    public static User toEntity(this CreateUserDTO user) {
        return new User {
            UserName = user.UserName,
            Password = user.Password,
            Role = "Default",
            Description = user.Description is null ? "" : user.Description
        };
    }

    public static User toEntity(this UpdateUserDTO user, User currentUser) {
        return new User {
            Id = currentUser.Id,
            UserName = user.UserName != null ? user.UserName : currentUser.UserName,
            Password = user.Password != null ? user.Password : currentUser.Password,
            Role = currentUser.Role,
            Description = user.Description is null ? "" : currentUser.Description
        };
    }

    public static UserDTO toDTO(this User user) {
        return new UserDTO {
            Id = user.Id,
            UserName = user.UserName,
            Role = user.Role,
            password = user.Password,
            Description = user.Description
        };
    }
}
