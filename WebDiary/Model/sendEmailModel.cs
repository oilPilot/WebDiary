
namespace WebDiary.Model;

public class sendEmailModel {
    public int? userId { get; set; }
    public string? CallbackUrl { get; set; }
    public bool IsValidation { get; set; } = false;
}
