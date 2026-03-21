using System.ComponentModel.DataAnnotations;

namespace WebDiary.DTO;

public record class GroupDTO
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string OwnerUserName { get; set; }
    public int OwnerId { get; set; }
    public string? PinCode { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public record class CreateGroupDTO
{
    [Required]
    public required string Name { get; set; }
    public int UserId { get; set; }
    public string? PinCode { get; set; }
}

public record class UpdateGroupDTO
{
    [Required]
    public required string Name { get; set; }
    public string? PinCode { get; set; }
    public bool IsArchived { get; set; }
}
