using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Localization;
using WebDiary.Frontend.Resources;

namespace WebDiary.Frontend.Models;

public class User {
    public int Id { get; set; }
    [MinLength(3)]
    [MaxLength(24)]
    public required string UserName { get; set; }
    public bool IsValidated { get; set; }
    [MinLength(8)]
    [MaxLength(64)]
    public string? Password { get; set; }
    public string? Description { get; set; } = "";
    public string? Role { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    [AllowNull]
    public byte[]? ActionToken { get; set; }
    [AllowNull]
    public DateTime? ActionDateEnd { get; set; }
}

