using System;

namespace WebDiary.Entities;

public class EntryReference
{
    public int Id { get; set; }
    
    // The entry that contains the reference
    public int SourceEntryId { get; set; }
    public Diary? SourceEntry { get; set; }
    
    // The entry being referenced
    public int ReferencedEntryId { get; set; }
    public Diary? ReferencedEntry { get; set; }
    
    // Optional: specific excerpt or part being referenced
    public string? ExcerptText { get; set; }
    
    // Start and end positions in referenced entry (for precise referencing)
    public int? ExcerptStartIndex { get; set; }
    public int? ExcerptEndIndex { get; set; }
    
    // Timestamp
    public DateTime CreatedAtUtc { get; set; }
}
