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
    public virtual DbSet<GroupPermission> groupPermissions => Set<GroupPermission>();
    public virtual DbSet<EntryReference> entryReferences => Set<EntryReference>();

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

        // Configure GroupPermission relationships
        modelBuilder.Entity<GroupPermission>(entity =>
        {
            entity.HasIndex(p => new { p.GroupId, p.UserId }).IsUnique();
            
            entity.HasOne(p => p.Group)
                .WithMany(g => g.Permissions)
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.User)
                .WithMany(u => u.GroupPermissions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.GrantedByUser)
                .WithMany()
                .HasForeignKey(p => p.GrantedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure EntryReference relationships
        modelBuilder.Entity<EntryReference>(entity =>
        {
            entity.HasOne(r => r.SourceEntry)
                .WithMany(d => d.ReferencesFromThisEntry)
                .HasForeignKey(r => r.SourceEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.ReferencedEntry)
                .WithMany(d => d.ReferencesTo)
                .HasForeignKey(r => r.ReferencedEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Diary-Owner relationship
        modelBuilder.Entity<Diary>(entity =>
        {
            entity.HasOne(d => d.Owner)
                .WithMany(u => u.OwnedEntries)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure DiaryGroup-Owner relationship
        modelBuilder.Entity<DiaryGroup>(entity =>
        {
            entity.HasOne(g => g.Owner)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}


