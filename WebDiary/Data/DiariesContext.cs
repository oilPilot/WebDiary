using System;
using Microsoft.EntityFrameworkCore;
using WebDiary.Entities;

namespace WebDiary.Data;

public class DiariesContext(DbContextOptions<DiariesContext> options) : DbContext(options)
{
    public DbSet<Diary> diaries => Set<Diary>();
    public DbSet<DiaryGroup> diaryGroups => Set<DiaryGroup>();
    public DbSet<User> users => Set<User>();
}
