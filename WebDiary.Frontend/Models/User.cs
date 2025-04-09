using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace WebDiary.Frontend.Models;

public class User {
    public int Id { get; set; }
    [MinLength(3)]
    public required string UserName { get; set; }
    public bool IsValidated { get; set; }
    [MinLength(8)]
    public string? Password { get; set; }
    public string? Description { get; set; } = "";
    public string? Role { get; set; }
    [EmailAddress(ErrorMessage = "This isn't correct Email address.")]
    public string? Email { get; set; }
    [AllowNull]
    public byte[]? ActionToken { get; set; }
    [AllowNull]
    public DateTime? ActionDateEnd { get; set; }
}

