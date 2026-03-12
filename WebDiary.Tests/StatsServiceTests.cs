using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebDiary.Data;
using WebDiary.DTO;
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

        [Fact]
        public async Task GetMostActiveUsersAsync_RespectLimit()
        {
            using var context = new DiariesContext(_options);
            var users = Enumerable.Range(0, 5)
                .Select(i => new User { UserName = $"user{i}", Password = "pw", Role = "Default", Description = "", IsValidated = true })
                .ToList();
            context.users.AddRange(users);
            context.SaveChanges();

            var groups = users
                .Select(u => new DiaryGroup { UserId = u.Id, Name = $"group_{u.UserName}" })
                .ToList();
            context.diaryGroups.AddRange(groups);
            context.SaveChanges();

            // Add diaries for each user
            foreach (var group in groups)
            {
                for (int i = 0; i < 3; i++)
                {
                    context.diaries.Add(new Diary
                    {
                        GroupId = group.Id,
                        Date = DateOnly.FromDateTime(DateTime.UtcNow),
                        BaseText = $"entry{i}",
                        Text = $"entry{i}",
                        mood = "happy",
                        CreatedUtc = DateTime.UtcNow,
                        Time = TimeOnly.FromDateTime(DateTime.UtcNow)
                    });
                }
            }
            context.SaveChanges();

            IStatsService service = new StatsService(context);
            var result = await service.GetMostActiveUsersAsync(3);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetNewUsersCountAsync_CountsUsersFromPeriod()
        {
            using var context = new DiariesContext(_options);
            var now = DateTime.UtcNow;
            var pastDate = now.AddDays(-10);

            var user1 = new User { UserName = "old_user", Password = "pw", Role = "Default", Description = "", IsValidated = true, CreatedAtUTC = pastDate.AddDays(-1) };
            var user2 = new User { UserName = "new_user1", Password = "pw", Role = "Default", Description = "", IsValidated = true, CreatedAtUTC = pastDate };
            var user3 = new User { UserName = "new_user2", Password = "pw", Role = "Default", Description = "", IsValidated = true, CreatedAtUTC = now };

            context.users.AddRange(user1, user2, user3);
            context.SaveChanges();

            IStatsService service = new StatsService(context);
            var result = await service.GetNewUsersCountAsync(pastDate);

            Assert.Equal(2, result); // user2 and user3
        }

        [Fact]
        public async Task GetNewEntriesCountAsync_CountsEntriesFromPeriod()
        {
            using var context = new DiariesContext(_options);
            var now = DateTime.UtcNow;
            var pastDate = now.AddDays(-10);

            var user = new User { UserName = "test_user", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            context.users.Add(user);
            context.SaveChanges();

            var group = new DiaryGroup { UserId = user.Id, Name = "test_group" };
            context.diaryGroups.Add(group);
            context.SaveChanges();

            // Old diary
            context.diaries.Add(new Diary
            {
                GroupId = group.Id,
                Date = DateOnly.FromDateTime(pastDate.AddDays(-1)),
                BaseText = "old",
                Text = "old",
                mood = "happy",
                CreatedUtc = pastDate.AddDays(-1),
                Time = TimeOnly.FromDateTime(pastDate.AddDays(-1))
            });

            // New diaries
            context.diaries.AddRange(
                new Diary { GroupId = group.Id, Date = DateOnly.FromDateTime(pastDate), BaseText = "new1", Text = "new1", mood = "happy", CreatedUtc = pastDate, Time = TimeOnly.FromDateTime(pastDate) },
                new Diary { GroupId = group.Id, Date = DateOnly.FromDateTime(now), BaseText = "new2", Text = "new2", mood = "sad", CreatedUtc = now, Time = TimeOnly.FromDateTime(now) }
            );
            context.SaveChanges();

            IStatsService service = new StatsService(context);
            var result = await service.GetNewEntriesCountAsync(pastDate);

            Assert.Equal(2, result);
        }

        [Fact]
        public async Task CountActiveUsersInPeriod_ReturnsCorrectCount()
        {
            using var context = new DiariesContext(_options);
            var now = DateTime.UtcNow;
            var pastDate = now.AddDays(-30);

            var user1 = new User { UserName = "active_user", Password = "pw", Role = "Default", Description = "", IsValidated = true, LastLoginAtUTC = pastDate };
            var user2 = new User { UserName = "inactive_user", Password = "pw", Role = "Default", Description = "", IsValidated = true, LastLoginAtUTC = pastDate.AddDays(-31) };
            var user3 = new User { UserName = "recent_user", Password = "pw", Role = "Default", Description = "", IsValidated = true, LastLoginAtUTC = now };

            context.users.AddRange(user1, user2, user3);
            context.SaveChanges();

            IStatsService service = new StatsService(context);
            var result = await service.CountActiveUsersInPeriod(pastDate);

            Assert.Equal(2, result); // user1 and user3
        }

        [Fact]
        public async Task GetNewInactiveUsersAsync_ReturnsInactiveUsers()
        {
            using var context = new DiariesContext(_options);
            var now = DateTime.UtcNow;
            var inactiveThreshold = now.AddDays(-30);

            var activeUser = new User { UserName = "active", Password = "pw", Role = "Default", Description = "", IsValidated = true, LastLoginAtUTC = now.AddDays(-10) };
            var inactiveUser1 = new User { UserName = "inactive1", Password = "pw", Role = "Default", Description = "", IsValidated = true, LastLoginAtUTC = inactiveThreshold.AddDays(-5) };
            var inactiveUser2 = new User { UserName = "inactive2", Password = "pw", Role = "Default", Description = "", IsValidated = true, LastLoginAtUTC = inactiveThreshold };

            context.users.AddRange(activeUser, inactiveUser1, inactiveUser2);
            context.SaveChanges();

            IStatsService service = new StatsService(context);
            var result = await service.GetNewInactiveUsersAsync(30);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.UserName == "inactive1");
            Assert.Contains(result, u => u.UserName == "inactive2");
        }

        [Fact]
        public async Task GetStatsForPeriod_ReturnsCorrectStats()
        {
            using var context = new DiariesContext(_options);
            var now = DateTime.UtcNow;
            var startDate = DateOnly.FromDateTime(now.AddDays(-7));
            var endDate = DateOnly.FromDateTime(now);

            var user = new User { UserName = "test_user", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            context.users.Add(user);
            context.SaveChanges();

            var group = new DiaryGroup { UserId = user.Id, Name = "test_group" };
            context.diaryGroups.Add(group);
            context.SaveChanges();

            var today = DateOnly.FromDateTime(now);
            var yesterday = today.AddDays(-1);

            context.diaries.AddRange(
                new Diary { GroupId = group.Id, Date = today, BaseText = "Entry1", Text = "Entry1", mood = "happy", CreatedUtc = DateTime.UtcNow, Time = TimeOnly.FromDateTime(DateTime.UtcNow) },
                new Diary { GroupId = group.Id, Date = today, BaseText = "Entry2", Text = "Entry2", mood = "sad", CreatedUtc = DateTime.UtcNow, Time = TimeOnly.FromDateTime(DateTime.UtcNow) },
                new Diary { GroupId = group.Id, Date = yesterday, BaseText = "YesterdayEntry", Text = "YesterdayEntry", mood = "neutral", CreatedUtc = DateTime.UtcNow.AddDays(-1), Time = TimeOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) }
            );
            context.SaveChanges();

            IStatsService service = new StatsService(context);
            var statsRequest = new StatsRequestDTO { StartDate = startDate, EndDate = endDate };
            var result = await service.GetStatsForPeriod(user.Id, statsRequest);

            Assert.NotEmpty(result);
            var todayStats = result.FirstOrDefault(s => s.Date == today);
            Assert.NotNull(todayStats);
            Assert.Equal(2, todayStats.EntriesCount);
        }

        [Fact]
        public async Task NewEntriesIn30DaysForChart_GeneratesDailyStats()
        {
            using var context = new DiariesContext(_options);
            var now = DateTime.UtcNow;

            // Add user and group
            var user = new User { UserName = "test_user", Password = "pw", Role = "Default", Description = "", IsValidated = true };
            context.users.Add(user);
            context.SaveChanges();

            var group = new DiaryGroup { UserId = user.Id, Name = "test_group" };
            context.diaryGroups.Add(group);
            context.SaveChanges();

            // Add entries for the last 7 days
            for (int i = 0; i < 7; i++)
            {
                context.diaries.Add(new Diary
                {
                    GroupId = group.Id,
                    Date = DateOnly.FromDateTime(now.AddDays(-i)),
                    BaseText = $"Entry{i}",
                    Text = $"Entry{i}",
                    mood = "happy",
                    CreatedUtc = now.AddDays(-i),
                    Time = TimeOnly.FromDateTime(now)
                });
            }
            context.SaveChanges();

            IStatsService service = new StatsService(context);
            var result = await service.NewEntriesIn30DaysForChart();

            Assert.NotEmpty(result);
            Assert.True(result.Count <= 31); // Should be max 31 days
            Assert.True(result.Any(s => s.EntriesCount > 0));
        }
    }
}