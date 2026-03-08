using System.ComponentModel.DataAnnotations;

namespace WebDiary.Model;

public class VerifyPasswordModel
{
    [Required]
    public string Password { get; set; } = "";
}
