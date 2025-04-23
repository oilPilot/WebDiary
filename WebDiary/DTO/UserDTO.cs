using System.ComponentModel.DataAnnotations;

namespace WebDiary.DTO;

public record class UserDTO
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required string Role { get; set; }
    public required string Description { get; set; }
    public required bool IsValidated { get; set; }
}

public record class CreateUserDTO
{
    [Required]
    public required string UserName { get; set; }
    [Required]
    public required string Password { get; set; }
    public string? Description { get; set; }
    public required string Email { get; set; }
    public byte[]? ActionToken { get; set; }
    public DateTime? ActionDateEnd { get; set; }
}

public record class UpdateUserDTO
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public bool? IsValidated { get; set; }
    public byte[]? ActionToken { get; set; }
    public DateTime? ActionDateEnd { get; set; }
}
