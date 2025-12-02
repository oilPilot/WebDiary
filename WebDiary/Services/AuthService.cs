using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Entities;
using WebDiary.Helpers;
using WebDiary.Mapping;


public class AuthService : IAuthService
{
    private readonly DiariesContext dbContext;
    private readonly IConfiguration config;

    public AuthService(DiariesContext dbContext, IConfiguration config)
    {
        this.dbContext = dbContext;
        this.config = config;
    }

    public async Task<PasswordVerificationResult> CheckPinEquality(string pin, int groupId)
    {
        var group = await dbContext.diaryGroups.FindAsync(groupId);
        if (group == null)
            throw new Exception("Not found group");
        if(string.IsNullOrEmpty(group.PinCode)) {
            return PasswordVerificationResult.Failed;
        }
        var hasher = new PasswordHasher<DiaryGroup>();
        return hasher.VerifyHashedPassword(group, group.PinCode, pin);
    }

    public async Task<PasswordVerificationResult> CheckPasswordEquality(string password, int userId)
    {
        var user = await dbContext.users.FindAsync(userId);
        if (user == null)
            throw new Exception("Not found group");
        var hasher = new PasswordHasher<User>();
        return hasher.VerifyHashedPassword(user, user.Password, password);
    }
    public async Task ResetPassword(User user, string newPassword)
    {
        var hasher = new PasswordHasher<User>();
        var newUser = user;
        newUser.Password = hasher.HashPassword(newUser, newPassword);
        newUser.ActionDateEnd = null;
        newUser.ActionToken = null;
        
        dbContext.users.Entry(user).CurrentValues.SetValues(newUser);
        await dbContext.SaveChangesAsync();
    }
    public async Task<string[]> CreatingTokens(User user, bool populateExpire = true) {
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        var oldUser = user;
        user.RefreshToken = refreshToken;
        if(populateExpire)
            user.RefreshTokenDateEnd = DateTime.UtcNow.AddDays(7);
        dbContext.users.Entry(oldUser).CurrentValues.SetValues(user);
        await dbContext.SaveChangesAsync();
        Log.Information("Created tokens for user {Name} with refresh token expiration {ExpTime}", user.UserName, user.RefreshTokenDateEnd);
        return new string[] {token, refreshToken};
    }

    // Helpers
    

    private string GenerateJwtToken(User user) {
        List<Claim> claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.UserData, user.Description)
            //,new Claim(ClaimTypes.Email, user.Email)
            };
        JwtSecurityToken securityToken = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256)
        );
        Log.Information("Generated jwt token for user {Name}", user.UserName);
        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
    private string GenerateRefreshToken() {
        var number = new byte[32];
        using(var random = RandomNumberGenerator.Create()) {
            random.GetBytes(number);

            return Convert.ToBase64String(number);
        }
    }
}
