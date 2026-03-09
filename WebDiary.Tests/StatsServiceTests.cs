using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebDiary.Data;
using WebDiary.Entities;
// StatsService and interface exist in the global namespace in the main project
using Xunit;

namespace WebDiary.Tests
{
    public class StatsServiceTests
    {
        private readonly DbContextOptions<DiariesContext> _options;

        public StatsServiceTests()
        {
            _options = new DbContextOptionsBuilder<DiariesContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetMostActiveUsersAsync_ReturnsOrderedList()
        {
            using var context = new DiariesContext(_options);
            // prepare users and diaries with groups
            var user1 = new User { UserName = "u1", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            var user2 = new User { UserName = "u2", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            context.users.AddRange(user1, user2);
            context.SaveChanges();
            var group1 = new DiaryGroup { UserId = user1.Id, Name = "g1" };
            var group2 = new DiaryGroup { UserId = user2.Id, Name = "g2" };
            context.diaryGroups.AddRange(group1, group2);
            context.SaveChanges();
            // add three diaries for user1 and one for user2
            context.diaries.AddRange(
                new Diary { GroupId = group1.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), BaseText = "a", Text = "a", mood = "happy", CreatedUtc = DateTime.UtcNow, Time = TimeOnly.FromDateTime(DateTime.UtcNow) },
                new Diary { GroupId = group1.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), BaseText = "b", Text = "b", mood = "sad", CreatedUtc = DateTime.UtcNow, Time = TimeOnly.FromDateTime(DateTime.UtcNow) },
                new Diary { GroupId = group1.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), BaseText = "c", Text = "c", mood = "neutral", CreatedUtc = DateTime.UtcNow, Time = TimeOnly.FromDateTime(DateTime.UtcNow) },
                new Diary { GroupId = group2.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), BaseText = "x", Text = "x", mood = "happy", CreatedUtc = DateTime.UtcNow, Time = TimeOnly.FromDateTime(DateTime.UtcNow) }
            );
            context.SaveChanges();

            IStatsService service = new StatsService(context);
            var result = await service.GetMostActiveUsersAsync(2);

            Assert.Equal(2, result.Count);
            Assert.Equal(user1.Id, result[0].UserId);
            Assert.True(result[0].EntriesCount >= result[1].EntriesCount);
        }
    }
}