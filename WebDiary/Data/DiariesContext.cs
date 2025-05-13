using System;
using Microsoft.EntityFrameworkCore;
using WebDiary.Entities;

namespace WebDiary.Data;

public class DiariesContext(DbContextOptions<DiariesContext> options) : DbContext(options)
{
    public virtual DbSet<Diary> diaries => Set<Diary>();
    public virtual DbSet<DiaryGroup> diaryGroups => Set<DiaryGroup>();
    public virtual DbSet<User> users => Set<User>();
}
