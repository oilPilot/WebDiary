
namespace WebDiary.Model;

public class resetPasswordModel {
    public required string Token { get; set; }
    public int UserId { get; set; }
    public required string newPassword { get; set; }
}
