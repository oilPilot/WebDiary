using System;

namespace WebDiary.Entities;

public class User
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
    public required string Description { get; set; }
    public required string Email { get; set; }
    public required bool IsValidated { get; set; }
    public byte[]? ActionToken { get; set; }
    public DateTime? ActionDateEnd { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenDateEnd { get; set; }
}
