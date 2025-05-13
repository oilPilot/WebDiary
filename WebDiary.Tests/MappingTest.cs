using Microsoft.AspNetCore.Identity;
using WebDiary.DTO;
using WebDiary.Entities;
using WebDiary.Mapping;

namespace WebDiary.Tests;

public class MappingTest
{
    [Fact]
    public void DiariesMapping_IsWorkingCorrect_ReturnsEqual()
    {
        // Arrange
        var diary = new Diary {
            Id = 1,
            Text = "Test",
            Date = new DateOnly(2024, 4, 1),
            Time = new TimeOnly(10, 0),
            GroupId = 5
        };
        var CreateDiary = new CreateDiaryDTO {
            Text = "Test2",
            GroupId = 5
        };
        
        // Act
        var toDto = diary.ToDTO();
        var toEntity = CreateDiary.ToEntity();

        // Assert
        Assert.Equal(diary.Text, toDto.Text);
        Assert.Equal(diary.Date, toDto.Date);
        Assert.Equal(diary.GroupId, toDto.GroupId);
        Assert.Equal(diary.Time, toDto.Time);
        Assert.Equal(CreateDiary.Text, toEntity.Text);
        Assert.Equal(CreateDiary.GroupId, toEntity.GroupId);
    }
    [Fact]
    public void GroupsMapping_IsWorkingCorrect_ReturnsEqual()
    {
        // Arrange
        var group = new DiaryGroup {
            Id = 1,
            Name = "NewGroup",
            UserId = 2
        };
        var CreateGroup = new CreateGroupDTO {
            Name = "CreateGroup",
            UserId = 2
        };
        var UpdateGroup = new UpdateGroupDTO {
            Name = "NewName"
        };
        
        // Act
        var toDto = group.toDTO();
        var toEntityUpdate = UpdateGroup.toEntity(group.Id, group.UserId);
        var toEntityCreate = CreateGroup.toEntity();

        // Assert
        Assert.Equal(group.Name, toDto.Name);
        Assert.Equal(group.Id, toDto.Id);
        Assert.Equal(CreateGroup.Name, toEntityCreate.Name);
        Assert.Equal(CreateGroup.UserId, toEntityCreate.UserId);
        Assert.Equal(UpdateGroup.Name, toEntityUpdate.Name);
    }
    [Fact]
    public void UsersMapping_IsWorkingCorrect_ReturnsEqual()
    {
        // Arrange
        var user = new User {
            UserName = "TestName",
            Password = "NewPswd",
            Role = "Default",
            Description = "Null",
            Email = "Sobaka@gmail.com",
            IsValidated = true
        };
        var CreateUser = new CreateUserDTO {
            UserName = "TestName24",
            Password = "Newj",
            Email = "New@gmail.com"
        };
        var UpdateUser = new UpdateUserDTO {
            UserName = "TestNameNew",
            Password = "Newjoy",
            Email = "Slasher@gmail.com",
        };
        var hasher = new PasswordHasher<User>();
        
        // Act
        var toDto = user.toDTO();
        var toEntityUpdate = UpdateUser.toEntity(user);
        var toEntityCreate = CreateUser.toEntity();

        // Assert
        //ToDTO
        Assert.Equal(user.Description, toDto.Description);
        Assert.Equal(user.UserName, toDto.UserName);
        Assert.Equal(user.Role, toDto.Role);
        Assert.Equal(user.IsValidated, toDto.IsValidated);
        //CreateUser
        Assert.Equal(CreateUser.UserName, toEntityCreate.UserName);
        hasher.VerifyHashedPassword(user, toEntityCreate.Password, CreateUser.Password);
        Assert.Equal(CreateUser.Email, toEntityCreate.Email);
        Assert.Equal(CreateUser.Description, toEntityCreate.Description);
        //UpdateUser
        if(UpdateUser.UserName != null) {
            Assert.Equal(UpdateUser.UserName, toEntityUpdate.UserName);
        } else {
            Assert.Equal(user.UserName, toEntityUpdate.UserName);
        }
        if(UpdateUser.Password != null) {
            hasher.VerifyHashedPassword(user, toEntityUpdate.Password, UpdateUser.Password);
        } else {
            hasher.VerifyHashedPassword(user, toEntityUpdate.Password, user.Password);
        }
        if(UpdateUser.Description != null) {
            Assert.Equal(UpdateUser.Description, toEntityUpdate.Description);
        } else {
            Assert.Equal(user.Description, toEntityUpdate.Description);
        }
        if(UpdateUser.Email != null) {
            Assert.Equal(UpdateUser.Email, toEntityUpdate.Email);
        } else {
            Assert.Equal(user.Email, toEntityUpdate.Email);
        }
        if(UpdateUser.IsValidated != null) {
            Assert.Equal(UpdateUser.IsValidated, toEntityUpdate.IsValidated);
        } else {
            Assert.Equal(user.IsValidated, toEntityUpdate.IsValidated);
        }
    }
}
