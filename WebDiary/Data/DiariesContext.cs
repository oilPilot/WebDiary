using System;
using Microsoft.EntityFrameworkCore;
using WebDiary.Entities;

namespace WebDiary.Data;

public class DiariesContext(DbContextOptions<DiariesContext> options) : DbContext(options)
{
    public virtual DbSet<Diary> diaries => Set<Diary>();
    public virtual DbSet<DiaryGroup> diaryGroups => Set<DiaryGroup>();
    public virtual DbSet<User> users => Set<User>();
    public virtual DbSet<LogRecord> logs => Set<LogRecord>();
    public virtual DbSet<ChatRoom> chatRooms => Set<ChatRoom>();
    public virtual DbSet<ChatMessage> chatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasIndex(room => room.Type);
            entity.Property(room => room.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasIndex(message => new { message.ChatRoomId, message.SentAtUtc });
            entity.Property(message => message.Content).HasMaxLength(2_000);
            entity.HasOne<ChatRoom>()
                .WithMany()
                .HasForeignKey(message => message.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

