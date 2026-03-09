using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebDiary.Entities;

public interface IAuthService
{
    Task<PasswordVerificationResult> CheckPinEquality(string pin, int groupId);
    Task<PasswordVerificationResult> CheckPasswordEquality(string password, int userId);
    Task<PasswordVerificationResult> CheckMasterPasswordEquality(string masterPassword, int userId);
    Task SetMasterPassword(int userId, string masterPassword);
    Task ResetPassword(User user, string newPassword);
    Task<string[]> CreatingTokens(User user, bool populateExpire = true);
}
