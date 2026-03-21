using System.ComponentModel.DataAnnotations;

namespace WebDiary.DTO;

public record class EntryReferenceDTO
{
    public int Id { get; set; }
    public int SourceEntryId { get; set; }
    public int ReferencedEntryId { get; set; }
    public string? ReferencedEntryOwner { get; set; }
    public string? ExcerptText { get; set; }
    public int? ExcerptStartIndex { get; set; }
    public int? ExcerptEndIndex { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public record class CreateEntryReferenceDTO
{
    [Required]
    public int SourceEntryId { get; set; }
    [Required]
    public int ReferencedEntryId { get; set; }
    public string? ExcerptText { get; set; }
    public int? ExcerptStartIndex { get; set; }
    public int? ExcerptEndIndex { get; set; }
}
