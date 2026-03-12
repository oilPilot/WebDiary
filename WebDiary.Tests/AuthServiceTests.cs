using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using WebDiary.Data;
using WebDiary.Entities;

namespace WebDiary.Tests
{
    public class AuthServiceTests
    {
        private readonly DbContextOptions<DiariesContext> _options;
        private readonly IConfiguration _config;

        public AuthServiceTests()
        {
            _options = new DbContextOptionsBuilder<DiariesContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"JwtSecret", "test_secret_key_that_is_long_enough"},
                {"JwtExpireMinutes", "60"}
            };
            
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task CheckPasswordEquality_WithCorrectPassword_ReturnsSuccess()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var user = new User
            {
                UserName = "testuser",
                Password = new PasswordHasher<User>().HashPassword(null!, "correctpassword"),
                Role = "Default",
                Description = "",
                IsValidated = true
            };
            context.users.Add(user);
            context.SaveChanges();

            var authService = new AuthService(context, _config);

            // Act
            var result = await authService.CheckPasswordEquality("correctpassword", user.Id);

            // Assert
            Assert.Equal(PasswordVerificationResult.Success, result);
        }

        [Fact]
        public async Task CheckPasswordEquality_WithIncorrectPassword_ReturnsFailure()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var user = new User
            {
                UserName = "testuser",
                Password = new PasswordHasher<User>().HashPassword(null!, "correctpassword"),
                Role = "Default",
                Description = "",
                IsValidated = true
            };
            context.users.Add(user);
            context.SaveChanges();

            var authService = new AuthService(context, _config);

            // Act
            var result = await authService.CheckPasswordEquality("wrongpassword", user.Id);

            // Assert
            Assert.Equal(PasswordVerificationResult.Failed, result);
        }

        [Fact]
        public async Task CheckPasswordEquality_WithNonexistentUser_ReturnsFailure()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var authService = new AuthService(context, _config);

            // Act
            var result = await authService.CheckPasswordEquality("anypassword", 999);

            // Assert
            Assert.Equal(PasswordVerificationResult.Failed, result);
        }

        [Fact]
        public async Task CheckPinEquality_WithCorrectPin_ReturnsSuccess()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var user = new User 
            { 
                UserName = "testuser", 
                Password = "pw", 
                Role = "Default", 
                Description = "", 
                IsValidated = true 
            };
            context.users.Add(user);
            context.SaveChanges();

            var group = new DiaryGroup 
            { 
                UserId = user.Id, 
                Name = "TestGroup",
                PinCode = new PasswordHasher<DiaryGroup>().HashPassword(null!, "1234")
            };
            context.diaryGroups.Add(group);
            context.SaveChanges();

            var authService = new AuthService(context, _config);

            // Act
            var result = await authService.CheckPinEquality("1234", group.Id);

            // Assert
            Assert.Equal(PasswordVerificationResult.Success, result);
        }

        [Fact]
        public async Task CheckPinEquality_WithIncorrectPin_ReturnsFailure()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var user = new User 
            { 
                UserName = "testuser", 
                Password = "pw", 
                Role = "Default", 
                Description = "", 
                IsValidated = true 
            };
            context.users.Add(user);
            context.SaveChanges();

            var group = new DiaryGroup 
            { 
                UserId = user.Id, 
                Name = "TestGroup",
                PinCode = new PasswordHasher<DiaryGroup>().HashPassword(null!, "1234")
            };
            context.diaryGroups.Add(group);
            context.SaveChanges();

            var authService = new AuthService(context, _config);

            // Act
            var result = await authService.CheckPinEquality("5678", group.Id);

            // Assert
            Assert.Equal(PasswordVerificationResult.Failed, result);
        }

        [Fact]
        public async Task CheckPinEquality_WithNullPin_ReturnsFailure()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var user = new User 
            { 
                UserName = "testuser", 
                Password = "pw", 
                Role = "Default", 
                Description = "", 
                IsValidated = true 
            };
            context.users.Add(user);
            context.SaveChanges();

            var group = new DiaryGroup 
            { 
                UserId = user.Id, 
                Name = "TestGroup",
                PinCode = null
            };
            context.diaryGroups.Add(group);
            context.SaveChanges();

            var authService = new AuthService(context, _config);

            // Act
            var result = await authService.CheckPinEquality("1234", group.Id);

            // Assert
            Assert.Equal(PasswordVerificationResult.Failed, result);
        }

        [Fact]
        public async Task CheckPinEquality_WithNonexistentGroup_ReturnsFailure()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var authService = new AuthService(context, _config);

            // Act
            var result = await authService.CheckPinEquality("1234", 999);

            // Assert
            Assert.Equal(PasswordVerificationResult.Failed, result);
        }

        [Fact]
        public async Task CheckMasterPasswordEquality_WithCorrectPassword_ReturnsSuccess()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var user = new User
            {
                UserName = "testuser",
                Password = "pw",
                Role = "Default",
                Description = "",
                IsValidated = true,
                MasterPassword = new PasswordHasher<User>().HashPassword(null!, "masterpassword123")
            };
            context.users.Add(user);
            context.SaveChanges();

            var authService = new AuthService(context, _config);

            // Act
            var result = await authService.CheckMasterPasswordEquality("masterpassword123", user.Id);

            // Assert
            Assert.Equal(PasswordVerificationResult.Success, result);
        }

        [Fact]
        public async Task CheckMasterPasswordEquality_WithIncorrectPassword_ReturnsFailure()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var user = new User
            {
                UserName = "testuser",
                Password = "pw",
                Role = "Default",
                Description = "",
                IsValidated = true,
                MasterPassword = new PasswordHasher<User>().HashPassword(null!, "masterpassword123")
            };
            context.users.Add(user);
            context.SaveChanges();

            var authService = new AuthService(context, _config);

            // Act
            var result = await authService.CheckMasterPasswordEquality("wrongmasterpassword", user.Id);

            // Assert
            Assert.Equal(PasswordVerificationResult.Failed, result);
        }

        [Fact]
        public async Task CheckMasterPasswordEquality_WithNullMasterPassword_ReturnsFailure()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var user = new User
            {
                UserName = "testuser",
                Password = "pw",
                Role = "Default",
                Description = "",
                IsValidated = true,
                MasterPassword = null
            };
            context.users.Add(user);
            context.SaveChanges();

            var authService = new AuthService(context, _config);

            // Act
            var result = await authService.CheckMasterPasswordEquality("anypassword", user.Id);

            // Assert
            Assert.Equal(PasswordVerificationResult.Failed, result);
        }

        [Fact]
        public async Task SetMasterPassword_WithValidUser_SetsMasterPassword()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var user = new User
            {
                UserName = "testuser",
                Password = "pw",
                Role = "Default",
                Description = "",
                IsValidated = true,
                MasterPassword = null
            };
            context.users.Add(user);
            context.SaveChanges();

            var authService = new AuthService(context, _config);
            var newMasterPassword = "newmasterpassword123";

            // Act
            await authService.SetMasterPassword(user.Id, newMasterPassword);

            // Assert
            var updatedUser = context.users.Find(user.Id);
            Assert.NotNull(updatedUser);
            Assert.NotNull(updatedUser.MasterPassword);
            
            // Verify the password was correctly hashed
            var verificationResult = new PasswordHasher<User>()
                .VerifyHashedPassword(updatedUser, updatedUser.MasterPassword, newMasterPassword);
            Assert.Equal(PasswordVerificationResult.Success, verificationResult);
        }

        [Fact]
        public async Task SetMasterPassword_UpdatesMasterPassword()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var oldPassword = "oldmasterpassword";
            var user = new User
            {
                UserName = "testuser",
                Password = "pw",
                Role = "Default",
                Description = "",
                IsValidated = true,
                MasterPassword = new PasswordHasher<User>().HashPassword(null!, oldPassword)
            };
            context.users.Add(user);
            context.SaveChanges();

            var authService = new AuthService(context, _config);
            var newMasterPassword = "newmasterpassword123";

            // Act
            await authService.SetMasterPassword(user.Id, newMasterPassword);

            // Assert
            var updatedUser = context.users.Find(user.Id);
            Assert.NotNull(updatedUser);
            
            // Verify old password no longer works
            var oldPasswordVerification = new PasswordHasher<User>()
                .VerifyHashedPassword(updatedUser, updatedUser.MasterPassword ?? "", oldPassword);
            Assert.NotEqual(PasswordVerificationResult.Success, oldPasswordVerification);
            
            // Verify new password works
            var newPasswordVerification = new PasswordHasher<User>()
                .VerifyHashedPassword(updatedUser, updatedUser.MasterPassword ?? "", newMasterPassword);
            Assert.Equal(PasswordVerificationResult.Success, newPasswordVerification);
        }

        [Fact]
        public async Task SetMasterPassword_WithNonexistentUser_DoesNotThrow()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var authService = new AuthService(context, _config);

            // Act & Assert - Should not throw
            await authService.SetMasterPassword(999, "anypassword");
        }

        [Fact]
        public async Task CheckPasswordEquality_MultipleTimes_ProducesConsistentResults()
        {
            // Arrange
            using var context = new DiariesContext(_options);
            var user = new User
            {
                UserName = "testuser",
                Password = new PasswordHasher<User>().HashPassword(null!, "correctpassword"),
                Role = "Default",
                Description = "",
                IsValidated = true
            };
            context.users.Add(user);
            context.SaveChanges();

            var authService = new AuthService(context, _config);

            // Act
            var result1 = await authService.CheckPasswordEquality("correctpassword", user.Id);
            var result2 = await authService.CheckPasswordEquality("correctpassword", user.Id);
            var result3 = await authService.CheckPasswordEquality("correctpassword", user.Id);

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
            Assert.Equal(PasswordVerificationResult.Success, result1);
        }
    }
}
