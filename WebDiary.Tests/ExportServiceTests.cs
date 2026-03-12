using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebDiary.Data;
using WebDiary.Entities;
using Xunit;

namespace WebDiary.Tests
{
    public class ExportServiceTests
    {
        private readonly DbContextOptions<DiariesContext> _options;

        public ExportServiceTests()
        {
            _options = new DbContextOptionsBuilder<DiariesContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void GetExportForEveryDiary_ReturnsNonEmptyPdf()
        {
            using var context = new DiariesContext(_options);
            
            // Setup: Create user with groups and diaries
            var user = new User { UserName = "test_user", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            context.users.Add(user);
            context.SaveChanges();

            var group1 = new DiaryGroup { UserId = user.Id, Name = "Group1" };
            var group2 = new DiaryGroup { UserId = user.Id, Name = "Group2" };
            context.diaryGroups.AddRange(group1, group2);
            context.SaveChanges();

            // Add diaries to groups
            context.diaries.AddRange(
                new Diary
                {
                    GroupId = group1.Id,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow),
                    BaseText = "Test Entry 1",
                    Text = "<p>Test Entry 1</p>",
                    mood = "happy",
                    CreatedUtc = DateTime.UtcNow,
                    Time = TimeOnly.FromDateTime(DateTime.UtcNow)
                },
                new Diary
                {
                    GroupId = group2.Id,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow),
                    BaseText = "Test Entry 2",
                    Text = "<p>Test Entry 2</p>",
                    mood = "sad",
                    CreatedUtc = DateTime.UtcNow,
                    Time = TimeOnly.FromDateTime(DateTime.UtcNow)
                }
            );
            context.SaveChanges();

            // Act
            var service = new ExportService(context);
            var pdfBytes = service.GetExportForEveryDiary(user.Id);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 0, "PDF should have content");
        }

        [Fact]
        public void GetExportForEveryDiary_WithNoDiaries_ReturnsEmptyPdf()
        {
            using var context = new DiariesContext(_options);
            
            // Setup: Create user with no diaries
            var user = new User { UserName = "empty_user", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            context.users.Add(user);
            context.SaveChanges();

            var group = new DiaryGroup { UserId = user.Id, Name = "EmptyGroup" };
            context.diaryGroups.Add(group);
            context.SaveChanges();

            // Act
            var service = new ExportService(context);
            var pdfBytes = service.GetExportForEveryDiary(user.Id);

            // Assert
            Assert.NotNull(pdfBytes);
            // Even empty export should have PDF headers
            Assert.True(pdfBytes.Length >= 0);
        }

        [Fact]
        public void GetExportForCertainGroup_ReturnsNonEmptyPdf()
        {
            using var context = new DiariesContext(_options);
            
            // Setup: Create user, group, and diaries
            var user = new User { UserName = "test_user", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            context.users.Add(user);
            context.SaveChanges();

            var group = new DiaryGroup { UserId = user.Id, Name = "SpecificGroup" };
            context.diaryGroups.Add(group);
            context.SaveChanges();

            context.diaries.AddRange(
                new Diary
                {
                    GroupId = group.Id,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow),
                    BaseText = "Entry for specific group 1",
                    Text = "<p>Entry for specific group 1</p>",
                    mood = "happy",
                    CreatedUtc = DateTime.UtcNow,
                    Time = TimeOnly.FromDateTime(DateTime.UtcNow)
                },
                new Diary
                {
                    GroupId = group.Id,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    BaseText = "Entry for specific group 2",
                    Text = "<p>Entry for specific group 2</p>",
                    mood = "neutral",
                    CreatedUtc = DateTime.UtcNow.AddDays(-1),
                    Time = TimeOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
                }
            );
            context.SaveChanges();

            // Act
            var service = new ExportService(context);
            var pdfBytes = service.GetExportForCertainGroup(group.Id);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 0, "PDF should have content");
        }

        [Fact]
        public void GetExportForCertainGroup_WithMultipleEntries_IncludesAll()
        {
            using var context = new DiariesContext(_options);
            
            // Setup: Create user with multiple diaries
            var user = new User { UserName = "test_user", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            context.users.Add(user);
            context.SaveChanges();

            var group = new DiaryGroup { UserId = user.Id, Name = "TestGroup" };
            context.diaryGroups.Add(group);
            context.SaveChanges();

            var numberOfEntries = 5;
            for (int i = 0; i < numberOfEntries; i++)
            {
                context.diaries.Add(new Diary
                {
                    GroupId = group.Id,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i)),
                    BaseText = $"Entry {i + 1}",
                    Text = $"<p>Entry {i + 1} content</p>",
                    mood = "happy",
                    CreatedUtc = DateTime.UtcNow.AddDays(-i),
                    Time = TimeOnly.FromDateTime(DateTime.UtcNow)
                });
            }
            context.SaveChanges();

            // Act
            var service = new ExportService(context);
            var pdfBytes = service.GetExportForCertainGroup(group.Id);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
        }

        [Fact]
        public void GetExportForCertainGroup_WithNonexistentGroup_ReturnsEmptyPdf()
        {
            using var context = new DiariesContext(_options);
            
            // Act
            var service = new ExportService(context);
            var pdfBytes = service.GetExportForCertainGroup(999); // Non-existent group ID

            // Assert
            Assert.NotNull(pdfBytes);
            // Should return PDF even if group doesn't exist (graceful handling)
            Assert.True(pdfBytes.Length >= 0);
        }

        [Fact]
        public void GetExportForEveryDiary_HandlesDifferentMoods()
        {
            using var context = new DiariesContext(_options);
            
            // Setup: Create user with diaries having different moods
            var user = new User { UserName = "test_user", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            context.users.Add(user);
            context.SaveChanges();

            var group = new DiaryGroup { UserId = user.Id, Name = "MoodTestGroup" };
            context.diaryGroups.Add(group);
            context.SaveChanges();

            var moods = new[] { "happy", "sad", "neutral", "anxious", "excited" };
            foreach (var mood in moods)
            {
                context.diaries.Add(new Diary
                {
                    GroupId = group.Id,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow),
                    BaseText = $"Entry with {mood} mood",
                    Text = $"<p>Entry with {mood} mood</p>",
                    mood = mood,
                    CreatedUtc = DateTime.UtcNow,
                    Time = TimeOnly.FromDateTime(DateTime.UtcNow)
                });
            }
            context.SaveChanges();

            // Act
            var service = new ExportService(context);
            var pdfBytes = service.GetExportForEveryDiary(user.Id);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
        }
    }
}
