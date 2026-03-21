using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebDiary.Entities;

public class Diary
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public required string BaseText { get; set; }
    // Local display parts
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    // Time created and Offset
    public DateTime CreatedUtc { get; set; }
    public int UtcOffsetMinutes { get; set; }
    
    public int GroupId { get; set; }
    public DiaryGroup? Group { get; set; }
    
    // Owner of this entry - tracks who created it
    public int OwnerId { get; set; }
    public User? Owner { get; set; }
    
    public required string mood { get; set; }
    
    // Navigation properties for entry references
    public ICollection<EntryReference> ReferencesFromThisEntry { get; set; } = new List<EntryReference>();
    public ICollection<EntryReference> ReferencesTo { get; set; } = new List<EntryReference>();
}
