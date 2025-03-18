using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace WebDiary.Frontend.Models;

public class User {
    public int Id { get; set; }
    [MinLength(3)]
    public required string UserName { get; set; }
    [MinLength(8)]
    public required string Password { get; set; }
    public required string Description { get; set; }
}

