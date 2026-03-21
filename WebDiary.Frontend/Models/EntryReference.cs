using System;

namespace WebDiary.Frontend.Models;

/// <summary>
/// Represents a reference from one diary entry to another
/// </summary>
public class EntryReference
{
    public int Id { get; set; }
    public int SourceEntryId { get; set; }
    public int ReferencedEntryId { get; set; }
    public string? ReferencedEntryOwner { get; set; }
    public string? ExcerptText { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>
/// Request to create a reference to another diary entry
/// </summary>
public class CreateEntryReferenceRequest
{
    public int ReferencedEntryId { get; set; }
    public string? ExcerptText { get; set; }
}
