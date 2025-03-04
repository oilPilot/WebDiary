using System.ComponentModel.DataAnnotations;

namespace WebDiary.DTO;

public record class UserDTO
{
    int Id { get; set; }
    public required string UserName { get; set; }
}
