using System;
using Microsoft.AspNetCore.Identity;
using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Mapping;

public static class Users
{
    public static User toEntity(this CreateUserDTO user) {
        var hasher = new PasswordHasher<User>();
        var dummyUser = new User() { UserName = "", Role = "", Description = "", Email = "", IsValidated = false, Password = "123" };
        return new User {
            UserName = user.UserName,
            Password = hasher.HashPassword(dummyUser, user.Password),
            Role = "Default",
            Email = user.Email is null ? "" : user.Email,
            Description = user.Description is null ? "" : user.Description,
            ActionToken = user.ActionToken,
            ActionDateEnd = user.ActionDateEnd,
            IsValidated = false
        };
    }

    public static User toEntity(this UpdateUserDTO user, User currentUser) {
        var hasher = new PasswordHasher<User>();
        return new User {
            Id = currentUser.Id,
            UserName = user.UserName != null ? user.UserName : currentUser.UserName,
            Password = user.Password != null ? hasher.HashPassword(currentUser, user.Password) : hasher.HashPassword(currentUser, currentUser.Password),
            Description = user.Description != null ? user.Description : currentUser.Description,
            Email = user.Email != null ? user.Email : currentUser.Email,
            ActionToken = user.ActionToken,
            ActionDateEnd = user.ActionDateEnd,
            Role = currentUser.Role,
            IsValidated = user.IsValidated != null ? user.IsValidated.Value : currentUser.IsValidated
        };
    }

    public static UserDTO toDTO(this User user) {
        return new UserDTO {
            Id = user.Id,
            UserName = user.UserName,
            Role = user.Role,
            Description = user.Description,
            IsValidated = user.IsValidated
        };
    }
}
