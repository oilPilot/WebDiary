using System.ComponentModel.DataAnnotations;

namespace WebDiary.DTO;

public record class GroupDTO
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int UserId { get; set; } = 1;
}

public record class CreateGroupDTO
{
    [Required]
    public required string Name { get; set; }
    public int UserId { get; set; }
}

public record class UpdateGroupDTO
{
    [Required]
    public required string Name { get; set; }
}
