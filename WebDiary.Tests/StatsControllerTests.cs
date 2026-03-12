using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebDiary.Controllers;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Entities;
using WebDiary.Model;
using Xunit;

namespace WebDiary.Tests
{
    public class StatsControllerTests
    {
        private readonly DbContextOptions<DiariesContext> _options;
        private readonly Mock<IStatsService> _mockStatsService;

        public StatsControllerTests()
        {
            _options = new DbContextOptionsBuilder<DiariesContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _mockStatsService = new Mock<IStatsService>();
        }

        private StatsController GetControllerWithUser(int userId, string role = "Default")
        {
            var controller = new StatsController(_mockStatsService.Object);
            
            var claims = new List<Claim>()
            {
                new Claim("userId", userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            
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
        public async Task GetForDates_WithValidUser_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var controller = GetControllerWithUser(userId);

            var expectedStats = new List<StatsDayDTO>
            {
                new StatsDayDTO 
                { 
                    Date = startDate, 
                    EntriesCount = 2, 
                    SymbolsCount = 100,
                    AvgSymbolsPerEntry = 50,
                    MoodPointChange = 10
                }
            };

            _mockStatsService
                .Setup(s => s.GetStatsForPeriod(userId, It.IsAny<StatsRequestDTO>()))
                .ReturnsAsync(expectedStats);

            // Act
            var result = await controller.GetForDates(startDate, endDate, userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetForDates_WithoutUserId_UsesCurrentUserId()
        {
            // Arrange
            var userId = 5;
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var controller = GetControllerWithUser(userId);

            var expectedStats = new List<StatsDayDTO>();

            _mockStatsService
                .Setup(s => s.GetStatsForPeriod(userId, It.IsAny<StatsRequestDTO>()))
                .ReturnsAsync(expectedStats);

            // Act
            var result = await controller.GetForDates(startDate, endDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockStatsService.Verify(s => s.GetStatsForPeriod(userId, It.IsAny<StatsRequestDTO>()), Times.Once);
        }

        [Fact]
        public async Task GetForDates_WithoutUserIdInClaims_ReturnsUnauthorized()
        {
            // Arrange
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
            
            var controller = new StatsController(_mockStatsService.Object);
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal() };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await controller.GetForDates(startDate, endDate);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetNewUsers_WithAdminRole_ReturnsOkResult()
        {
            // Arrange
            var controller = GetControllerWithUser(1, "Admin");
            var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

            _mockStatsService
                .Setup(s => s.GetNewUsersCountAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(5);

            // Act
            var result = await controller.GetNewUsers(fromDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(5, okResult.Value);
        }

        [Fact]
        public async Task GetNewEntries_WithAdminRole_ReturnsOkResult()
        {
            // Arrange
            var controller = GetControllerWithUser(1, "Admin");
            var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

            _mockStatsService
                .Setup(s => s.GetNewEntriesCountAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(15);

            // Act
            var result = await controller.GetNewEntries(fromDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(15, okResult.Value);
        }

        [Fact]
        public async Task GetActiveUsers_WithAdminRole_ReturnsOkResult()
        {
            // Arrange
            var controller = GetControllerWithUser(1, "Admin");
            var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

            _mockStatsService
                .Setup(s => s.CountActiveUsersInPeriod(It.IsAny<DateTime>()))
                .ReturnsAsync(20);

            // Act
            var result = await controller.GetActiveUsers(fromDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(20, okResult.Value);
        }

        [Fact]
        public async Task GetInactiveUsers_WithAdminRole_ReturnsOkResult()
        {
            // Arrange
            var controller = GetControllerWithUser(1, "Admin");

            var inactiveUsers = new List<UserDTO>
            {
                new UserDTO { Id = 2, UserName = "inactive1", Role = "Default", Description = "", IsValidated = true },
                new UserDTO { Id = 3, UserName = "inactive2", Role = "Default", Description = "", IsValidated = true }
            };

            _mockStatsService
                .Setup(s => s.GetNewInactiveUsersAsync(It.IsAny<int>()))
                .ReturnsAsync(inactiveUsers);

            // Act
            var result = await controller.GetInactiveUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsType<List<UserDTO>>(okResult.Value);
            Assert.Equal(2, returnedList.Count);
        }

        [Fact]
        public async Task Get30DaysStats_WithAdminRole_ReturnsOkResult()
        {
            // Arrange
            var controller = GetControllerWithUser(1, "Admin");

            var stats = new List<AdminStatsModel>
            {
                new AdminStatsModel { DateOfData = DateOnly.FromDateTime(DateTime.UtcNow), EntriesCount = 5, SymbolsCount = 500 },
                new AdminStatsModel { DateOfData = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), EntriesCount = 3, SymbolsCount = 300 }
            };

            _mockStatsService
                .Setup(s => s.NewEntriesIn30DaysForChart())
                .ReturnsAsync(stats);

            // Act
            var result = await controller.Get30DaysStats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsType<List<AdminStatsModel>>(okResult.Value);
            Assert.Equal(2, returnedList.Count);
        }

        [Fact]
        public async Task GetMostActiveUsers_WithAdminRole_ReturnsOkResult()
        {
            // Arrange
            var controller = GetControllerWithUser(1, "Admin");

            var activeUsers = new List<UserActivityDTO>
            {
                new UserActivityDTO { UserId = 1, UserName = "user1", EntriesCount = 50 },
                new UserActivityDTO { UserId = 2, UserName = "user2", EntriesCount = 40 },
                new UserActivityDTO { UserId = 3, UserName = "user3", EntriesCount = 30 }
            };

            _mockStatsService
                .Setup(s => s.GetMostActiveUsersAsync(It.IsAny<int>()))
                .ReturnsAsync(activeUsers);

            // Act
            var result = await controller.GetMostActiveUsers(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsType<List<UserActivityDTO>>(okResult.Value);
            Assert.Equal(3, returnedList.Count);
        }

        [Fact]
        public async Task GetMostActiveUsers_WithCustomLimit_ReturnsLimitedResults()
        {
            // Arrange
            var controller = GetControllerWithUser(1, "Admin");
            var limit = 2;

            var activeUsers = new List<UserActivityDTO>
            {
                new UserActivityDTO { UserId = 1, UserName = "user1", EntriesCount = 50 },
                new UserActivityDTO { UserId = 2, UserName = "user2", EntriesCount = 40 }
            };

            _mockStatsService
                .Setup(s => s.GetMostActiveUsersAsync(limit))
                .ReturnsAsync(activeUsers);

            // Act
            var result = await controller.GetMostActiveUsers(limit);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockStatsService.Verify(s => s.GetMostActiveUsersAsync(limit), Times.Once);
        }

        [Fact]
        public async Task GetForDates_WithDateRange_CallsServiceWithCorrectDates()
        {
            // Arrange
            var userId = 1;
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);
            var controller = GetControllerWithUser(userId);

            _mockStatsService
                .Setup(s => s.GetStatsForPeriod(userId, It.IsAny<StatsRequestDTO>()))
                .ReturnsAsync(new List<StatsDayDTO>());

            // Act
            await controller.GetForDates(startDate, endDate, userId);

            // Assert
            _mockStatsService.Verify(
                s => s.GetStatsForPeriod(userId, It.Is<StatsRequestDTO>(dto =>
                    dto.StartDate == startDate && dto.EndDate == endDate)),
                Times.Once);
        }
    }
}
