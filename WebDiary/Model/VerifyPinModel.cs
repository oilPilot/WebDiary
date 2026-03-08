using System.ComponentModel.DataAnnotations;

namespace WebDiary.Model;

public class VerifyPinModel
{
    [Required]
    public string Pin { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int GroupId { get; set; }
}
