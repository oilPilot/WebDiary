using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebDiary.Data;
using WebDiary.Entities;
using Xunit;

namespace WebDiary.Tests
{
    public class ExportControllerTests
    {
        private readonly DbContextOptions<DiariesContext> _options;
        private readonly Mock<IExportService> _mockExportService;
        private readonly DiariesContext _dbContext;

        public ExportControllerTests()
        {
            _options = new DbContextOptionsBuilder<DiariesContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _mockExportService = new Mock<IExportService>();
            _dbContext = new DiariesContext(_options);
        }

        private ExportController GetControllerWithUser(int userId, string role = "Default", bool isAdmin = false)
        {
            var controller = new ExportController(_mockExportService.Object, _dbContext);
            
            var claims = new List<Claim>()
            {
                new Claim("userId", userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            };

            if (isAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            return controller;
        }

        [Fact]
        public void ExportAllDiaryGroup_WithValidUser_ReturnsFile()
        {
            // Arrange
            var userId = 1;
            var controller = GetControllerWithUser(userId, "Default");
            
            var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
            _mockExportService
                .Setup(s => s.GetExportForEveryDiary(userId))
                .Returns(pdfContent);

            // Act
            var result = controller.ExportAllDiaryGroup(userId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal("MyDiary.pdf", fileResult.FileDownloadName);
            Assert.Equal(pdfContent, fileResult.FileContents);
        }

        [Fact]
        public void ExportAllDiaryGroup_WithoutUserIdInClaims_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new ExportController(_mockExportService.Object, _dbContext);
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal() };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = controller.ExportAllDiaryGroup(1);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void ExportAllDiaryGroup_WithAdminUser_CanExportOtherUserData()
        {
            // Arrange
            var adminId = 1;
            var targetUserId = 2;
            var controller = GetControllerWithUser(adminId, isAdmin: true);
            
            var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _mockExportService
                .Setup(s => s.GetExportForEveryDiary(targetUserId))
                .Returns(pdfContent);

            // Act
            var result = controller.ExportAllDiaryGroup(targetUserId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.NotNull(fileResult);
        }

        [Fact]
        public void ExportAllDiaryGroup_WithNonAdminUser_CannotExportOthersData()
        {
            // Arrange
            var userId = 1;
            var otherUserId = 2;
            var controller = GetControllerWithUser(userId, "Default");

            // Act
            var result = controller.ExportAllDiaryGroup(otherUserId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task ExportCertainDiaryGroup_WithValidUser_ReturnsFile()
        {
            // Arrange
            var userId = 1;
            var controller = GetControllerWithUser(userId, "Default");

            // Setup database with valid group ownership
            var user = new User { UserName = "test", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            _dbContext.users.Add(user);
            _dbContext.SaveChanges();

            var group = new DiaryGroup { UserId = user.Id, Name = "TestGroup" };
            _dbContext.diaryGroups.Add(group);
            _dbContext.SaveChanges();

            // Mock export service
            var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _mockExportService
                .Setup(s => s.GetExportForCertainGroup(group.Id))
                .Returns(pdfContent);

            // Update controller with correct user
            var updatedController = GetControllerWithUser(user.Id, "Default");

            // Act
            var result = await updatedController.ExportCertainDiaryGroup(group.Id);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal("MyDiary.pdf", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportCertainDiaryGroup_WithoutUserIdInClaims_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new ExportController(_mockExportService.Object, _dbContext);
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal() };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await controller.ExportCertainDiaryGroup(1);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task ExportCertainDiaryGroup_WithAdminRole_CanExportAnyGroup()
        {
            // Arrange
            var adminId = 1;
            var groupId = 1;
            var controller = GetControllerWithUser(adminId, isAdmin: true);

            var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _mockExportService
                .Setup(s => s.GetExportForCertainGroup(groupId))
                .Returns(pdfContent);

            // Act
            var result = await controller.ExportCertainDiaryGroup(groupId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
        }

        [Fact]
        public async Task ExportCertainDiaryGroup_WithNonOwnerUser_ReturnsForibid()
        {
            // Arrange
            var userId = 1;
            var controller = GetControllerWithUser(userId, "Default");
            var groupId = 999; // Group not owned by user

            // Act
            var result = await controller.ExportCertainDiaryGroup(groupId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void ExportAllDiaryGroup_ReturnsCorrectContentType()
        {
            // Arrange
            var userId = 1;
            var controller = GetControllerWithUser(userId, "Default");
            
            var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _mockExportService
                .Setup(s => s.GetExportForEveryDiary(userId))
                .Returns(pdfContent);

            // Act
            var result = controller.ExportAllDiaryGroup(userId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
        }

        [Fact]
        public void ExportAllDiaryGroup_ReturnsCorrectFileName()
        {
            // Arrange
            var userId = 1;
            var controller = GetControllerWithUser(userId, "Default");
            
            var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _mockExportService
                .Setup(s => s.GetExportForEveryDiary(userId))
                .Returns(pdfContent);

            // Act
            var result = controller.ExportAllDiaryGroup(userId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("MyDiary.pdf", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportCertainDiaryGroup_ReturnsCorrectFileName()
        {
            // Arrange
            var userId = 1;
            var controller = GetControllerWithUser(userId, "Default");

            // Setup database
            var user = new User { UserName = "test", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            _dbContext.users.Add(user);
            _dbContext.SaveChanges();

            var group = new DiaryGroup { UserId = user.Id, Name = "TestGroup" };
            _dbContext.diaryGroups.Add(group);
            _dbContext.SaveChanges();

            var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _mockExportService
                .Setup(s => s.GetExportForCertainGroup(group.Id))
                .Returns(pdfContent);

            var updatedController = GetControllerWithUser(user.Id, "Default");

            // Act
            var result = await updatedController.ExportCertainDiaryGroup(group.Id);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("MyDiary.pdf", fileResult.FileDownloadName);
        }
    }
}
