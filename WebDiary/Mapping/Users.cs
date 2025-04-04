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
            Email = user.Email is null ? "" : user.Email,
            Description = user.Description is null ? "" : user.Description
        };
    }

    public static User toEntity(this UpdateUserDTO user, User currentUser) {
        return new User {
            Id = currentUser.Id,
            UserName = user.UserName != null ? user.UserName : currentUser.UserName,
            Password = user.Password != null ? user.Password : currentUser.Password,
            Description = user.Description != null ? user.Description : currentUser.Description,
            Email = user.Email != null ? user.Email : currentUser.Email,
            ResetPasswordToken = user.ResetPasswordToken,
            ResetPasswordDateEnd = user.ResetPasswordDateEnd,
            Role = currentUser.Role
        };
    }

    public static UserDTO toDTO(this User user) {
        return new UserDTO {
            Id = user.Id,
            UserName = user.UserName,
            Role = user.Role,
            Description = user.Description
        };
    }
}
