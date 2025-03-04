using System;

namespace WebDiary.Entities;

public class User
{
    public int Id { get; set; }
    public required string UserName { get; set; }
}
