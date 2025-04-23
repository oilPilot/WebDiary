using System;
using System.ComponentModel.DataAnnotations;

namespace WebDiary.Model;

public class LoginModel
{
    [Required]
    public string? Username { get; set; } = "";
    [Required]
    public string? Password { get; set; } = "";
}
